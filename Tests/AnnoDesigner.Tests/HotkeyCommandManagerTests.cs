﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using System.Reflection;
using Moq;
using Xunit;
using AnnoDesigner.Localization;

namespace AnnoDesigner.Tests
{
    public class HotkeyCommandManagerTests
    {
        public HotkeyCommandManagerTests() { }

        private static readonly Action<object> emptyAction = (o) => { };
        private static readonly RelayCommand emptyCommand = new RelayCommand(emptyAction);

        /// <summary>
        /// Returns a <see cref="HotkeyCommandManager"/> instance, a <see cref="string"/> for an id, and a <see cref="PolyBinding{T}"/>.
        /// </summary>
        /// <param name="addBinding">Set to true to add the binding to the <see cref="HotkeyCommandManager{T}"/> instance</param>
        /// <returns></returns>
        private static (HotkeyCommandManager, string, KeyBinding) GetDefaultSetup(bool addBinding)
        {

            var hotkeyCommandManager = GetMockedHotkeyCommandManager();
            var id = "hotkey";
            var binding = GetInputBinding(Key.A);
            if (addBinding)
            {
                hotkeyCommandManager.AddHotkey(id, binding);
            }
            return (hotkeyCommandManager, id, binding);
        }

        public static HotkeyCommandManager GetMockedHotkeyCommandManager()
        {
            var localizationDictionary = new Dictionary<string, string>();
            var mockedLocalization = new Mock<ILocalization>();
            mockedLocalization.Setup(m => m.InstanceTranslations).Returns(localizationDictionary);
            return new HotkeyCommandManager(mockedLocalization.Object);
        }

        // The following code errors:
        /*
            //exception -> Can't create a KeyGesture with single key and no modifiers.
            //InputBindings.Add(new KeyBinding(_viewModel.ShowMessageCommand, new KeyGesture(Key.D, ModifierKeys.None)));
            //InputBindings.Add(new KeyBinding(_viewModel.ShowMessageCommand, Key.D, ModifierKeys.None));
            //InputBindings.Add(new InputBinding(_viewModel.ShowMessageCommand, new KeyGesture(Key.D, ModifierKeys.None)));

            See this PR thread for details: https://github.com/AnnoDesigner/anno-designer/pull/191#issuecomment-632695207
         */

        private static KeyBinding GetInputBinding(Key key)
        {
            return GetInputBinding(key, ModifierKeys.None);
        }
        private static MouseBinding GetInputBinding(MouseAction mouseAction)
        {
            return new MouseBinding
            {
                Command = emptyCommand,
                Gesture = new MouseGesture()
                {
                    MouseAction = mouseAction,
                    Modifiers = ModifierKeys.None
                }
            };
        }

        private static MouseBinding GetInputBinding(MouseAction mouseAction, ModifierKeys modifiers)
        {
            return new MouseBinding
            {
                Command = emptyCommand,
                Gesture = new MouseGesture()
                {
                    MouseAction = mouseAction,
                    Modifiers = modifiers
                }
            };
        }

        private static KeyBinding GetInputBinding(Key key, ModifierKeys modifierKeys)
        {
            return new KeyBinding
            {
                Command = emptyCommand,
                Key = key,
                Modifiers = modifierKeys
            };
        }

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            //Arrange
            var hotkeyCommandManager = GetMockedHotkeyCommandManager();

            //Act
            var bindings = hotkeyCommandManager.GetHotkeys();

            //Assert
            Assert.NotNull(hotkeyCommandManager.ObservableCollection);
            Assert.NotNull(bindings);
            Assert.Empty(bindings);
        }

        public static IEnumerable<object[]> NewBindingData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[] { "Keybind1", GetInputBinding(Key.A) },
                    new object[] { "Keybind2", GetInputBinding(Key.B, ModifierKeys.Alt | ModifierKeys.Control) },
                    new object[] { "Keybind3", GetInputBinding(Key.C, ModifierKeys.Shift) },
                    new object[] { "Keybind4", GetInputBinding(MouseAction.LeftClick) },
                    new object[] { "Keybind5", GetInputBinding(MouseAction.RightDoubleClick, ModifierKeys.Control) }
                };
            }
        }
        [Theory]
        [MemberData(nameof(NewBindingData))]
        //Modifiers
        public void AddHotkey_NewItemAdded_ShouldSyncToObservableCollection(string id,  InputBinding expectedBinding)
        {
            //Arrange
            var hotkeyCommandManager = GetMockedHotkeyCommandManager();

            //Act
            hotkeyCommandManager.AddHotkey(id, expectedBinding);
            hotkeyCommandManager.AddHotkey(id + "a", expectedBinding);
            hotkeyCommandManager.AddHotkey(id + "b", expectedBinding);

            var hotkey = hotkeyCommandManager.ObservableCollection.First();

            //Assert
            Assert.Equal(3, hotkeyCommandManager.ObservableCollection.Count);
            Assert.Same(expectedBinding, hotkey.Binding);
            if (hotkey.Binding is KeyBinding actualKeyBinding)
            {
                var expectedKeyBinding = expectedBinding as KeyBinding;
                Assert.Equal(expectedKeyBinding.Key , actualKeyBinding.Key);
                Assert.Equal(expectedKeyBinding.Modifiers , actualKeyBinding.Modifiers);
               
            }
            else
            {
                var actualMouseBinding = hotkey.Binding as MouseBinding;
                var expectedMouseBinding = expectedBinding as MouseBinding;
                Assert.Equal(expectedMouseBinding.MouseAction, actualMouseBinding.MouseAction);
                Assert.Equal((expectedMouseBinding.Gesture as MouseGesture).Modifiers, (actualMouseBinding.Gesture as MouseGesture).Modifiers);
            }

        }

        public static IEnumerable<object[]> UpdateBindingData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[] 
                    { 
                        "Keybind1", 
                        GetInputBinding(Key.A), 
                        Key.B,
                        ModifierKeys.None, 
                        default(MouseAction) 
                    },
                    new object[] 
                    { 
                        "Keybind2", 
                        GetInputBinding(Key.C, ModifierKeys.Control | ModifierKeys.Alt), 
                        Key.D, 
                        ModifierKeys.Control, 
                        default(MouseAction)
                    },
                    new object[] 
                    { 
                        "Keybind3", 
                        GetInputBinding(MouseAction.LeftDoubleClick), 
                        default(Key), 
                        ModifierKeys.Alt, 
                        MouseAction.MiddleDoubleClick
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(MouseAction.RightClick, ModifierKeys.Shift),
                        Key.F,
                        ModifierKeys.Control | ModifierKeys.Alt,
                        default(MouseAction)
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(UpdateBindingData))]
        public void UpdateHotkey_KeyOrMouseUpdated_ShouldSyncToObservableCollection(string id, InputBinding expectedBinding, Key expectedKey, ModifierKeys expectedModifierKeys, MouseAction expectedMouseAction)
        {
            //Arrange
            var hotkeyCommandManager = GetMockedHotkeyCommandManager();

            //Act
            hotkeyCommandManager.AddHotkey(id, expectedBinding);

            if (expectedBinding is KeyBinding keyBinding)
            {
                //change binding from a KeyBinding to a MouseBinding
                if (expectedMouseAction != default)
                {
                    var hotkey = hotkeyCommandManager.GetHotkey(id);
                    hotkey.Binding = new MouseBinding(emptyCommand, new MouseGesture(expectedMouseAction, expectedModifierKeys));
                    expectedBinding = hotkey.Binding;
                }
                else
                {
                    keyBinding.Key = expectedKey;
                    keyBinding.Modifiers = expectedModifierKeys;
                }
            }
            else
            {
                var mouseBinding = expectedBinding as MouseBinding;

                //Change binding from a MouseBinding to a KeyBinding
                if (expectedKey != default)
                {
                    var hotkey = hotkeyCommandManager.GetHotkey(id);
                    hotkey.Binding = new KeyBinding
                    {
                        Command = emptyCommand,
                        Key = expectedKey,
                        Modifiers = expectedModifierKeys
                    };
                    expectedBinding = hotkey.Binding;
                }
                else
                {
                    //Can't set the Modifiers property without creating a new MouseGesture.
                    mouseBinding.Gesture = new MouseGesture(expectedMouseAction, expectedModifierKeys);
                }
            }

            var actualBinding = hotkeyCommandManager.ObservableCollection.First().Binding;

            //Assert

            Assert.Same(expectedBinding, actualBinding);
            if (actualBinding is KeyBinding actualKeyBinding)
            {
                var expectedKeyBinding = expectedBinding as KeyBinding;
                Assert.Equal(expectedKeyBinding.Key, actualKeyBinding.Key);
                Assert.Equal(expectedKeyBinding.Modifiers, actualKeyBinding.Modifiers);
            }
            else
            {
                var actualMouseBinding = actualBinding as MouseBinding;
                var expectedMouseBinding = expectedBinding as MouseBinding;
                Assert.Equal(expectedMouseBinding.MouseAction, actualMouseBinding.MouseAction);
                Assert.Equal((expectedMouseBinding.Gesture as MouseGesture).Modifiers, (actualMouseBinding.Gesture as MouseGesture).Modifiers);
            }
        }

        [Fact]
        public void RemoveHotkey_ShouldSyncToObservableCollection()
        {
            //Arrange
            var (hotkeyCommandManager, id, _) = GetDefaultSetup(true);

            //Act
            hotkeyCommandManager.RemoveHotkey(id);

            //Assert
            Assert.Empty(hotkeyCommandManager.ObservableCollection);
        }

        [Fact]
        public void AddHotkey_Duplicate_ShouldThrowArgumentException()
        {
            //Arrange
            var (hotkeyCommandManager, id, binding) = GetDefaultSetup(true);
            //Act and assert
            Assert.Throws<ArgumentException>(() => hotkeyCommandManager.AddHotkey(id, binding));
        }

        [Fact]
        public void RemoveHotkey_NonExistentBindingId_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            var (hotkeyCommandManager, id, binding) = GetDefaultSetup(true);
            //Act and assert
            Assert.Throws<KeyNotFoundException>(() => hotkeyCommandManager.RemoveHotkey(""));
        }

        [Fact]
        public void GetHotkey_NonExistentBindingId_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            var (hotkeyCommandManager, id, binding) = GetDefaultSetup(false);
            //Act and assert
            Assert.Throws<KeyNotFoundException>(() => hotkeyCommandManager.GetHotkey(id));
        }

        [Fact]
        public void LoadHotkeyMappings_NullArgument_DoesNotThrowException()
        {
            //Arrange
            var hotkeyCommandMananger = GetMockedHotkeyCommandManager();
            //Act and Assert
            hotkeyCommandMananger.LoadHotkeyMappings(null);
            Assert.True(true);
        }        
        
        [Fact]
        public void LoadHotkeyMappings_InvalidType_IgnoresMapping()
        {
            //Arrange
            var hotkeyCommandMananger = GetMockedHotkeyCommandManager();
            var newMappings = new Dictionary<string, HotkeyInformation>()
            {
                { "myHotkeyInfo", new HotkeyInformation(Key.A, default, ModifierKeys.None, typeof(HotkeyCommandManager)) }
            };
            //Act
            hotkeyCommandMananger.LoadHotkeyMappings(newMappings);
            hotkeyCommandMananger.AddHotkey("myHotkeyInfo", new KeyBinding(emptyCommand, Key.C, ModifierKeys.Control));
            //Assert
            var keyBinding= hotkeyCommandMananger.GetHotkey("myHotkeyInfo").Binding as KeyBinding;
            Assert.Equal(Key.C, keyBinding.Key);
            Assert.Equal(ModifierKeys.Control, keyBinding.Modifiers);
        }

        public static IEnumerable<object[]> HotkeyMappingLoadData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        "Keybind1",
                        GetInputBinding(Key.A),
                        Key.A,
                        ModifierKeys.None,
                        default(MouseAction),
                        typeof(KeyBinding)
                    },
                    new object[]
                    {
                        "Keybind2",
                        GetInputBinding(Key.C, ModifierKeys.Control | ModifierKeys.Alt),
                        Key.D,
                        ModifierKeys.Control,
                        default(MouseAction),
                        typeof(KeyBinding)
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(MouseAction.LeftDoubleClick),
                        default(Key),
                        ModifierKeys.Alt,
                        MouseAction.MiddleDoubleClick,
                        typeof(MouseBinding)
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(MouseAction.RightClick, ModifierKeys.Shift),
                        Key.F,
                        ModifierKeys.Control | ModifierKeys.Alt,
                        default(MouseAction),
                        typeof(KeyBinding)
                    },                    
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(MouseAction.RightClick, ModifierKeys.Shift),
                        default(Key),
                        ModifierKeys.Shift,
                        MouseAction.RightClick,
                        typeof(MouseBinding)
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(HotkeyMappingLoadData))]
        public void LoadHotkeyMappings_CalledAfterHotkeyIsAdded_LoadsNewHotkeyMappings(string hotkeyId, InputBinding binding, Key expectedKey, ModifierKeys expectedModifiers, MouseAction expectedMouseAction, Type expectedType)
        {
            //Arrange
            var hotkeyCommandManager = GetMockedHotkeyCommandManager();

            var newMappings = new Dictionary<string, HotkeyInformation>()
            {
                { hotkeyId, new HotkeyInformation(expectedKey, expectedMouseAction, expectedModifiers, expectedType) }
            };

            //Act
            hotkeyCommandManager.AddHotkey(hotkeyId, binding);
            var hotkey = hotkeyCommandManager.GetHotkey(hotkeyId);
            hotkeyCommandManager.LoadHotkeyMappings(newMappings); //Call LoadHotkeyMappings after hotkey is added

            //Assert
            var actualHotkey = hotkeyCommandManager.GetHotkey(hotkeyId);

            Assert.Same(hotkey, actualHotkey);
            if (actualHotkey.Binding is KeyBinding actualKeyBinding)
            {
                Assert.Equal(expectedType, actualHotkey.Binding.GetType());
                Assert.Equal(expectedKey, actualKeyBinding.Key);
                Assert.Equal(expectedModifiers, actualKeyBinding.Modifiers);
            }
            else
            {
                var actualMouseBinding = actualHotkey.Binding as MouseBinding;
                Assert.Equal(expectedType, actualHotkey.Binding.GetType());
                Assert.Equal(expectedMouseAction, actualMouseBinding.MouseAction);
                Assert.Equal(expectedModifiers, (actualMouseBinding.Gesture as MouseGesture).Modifiers);
            }

        }

        [Theory]
        [MemberData(nameof(HotkeyMappingLoadData))]
        public void LoadHotkeyMappings_CalledBeforeHotkeyIsAdded_LoadsNewHotkeyMappings(string hotkeyId, InputBinding binding, Key expectedKey, ModifierKeys expectedModifiers, MouseAction expectedMouseAction, Type expectedType)
        {
            //Arrange
            var hotkeyCommandManager = GetMockedHotkeyCommandManager();

            var newMappings = new Dictionary<string, HotkeyInformation>()
            {
                { hotkeyId, new HotkeyInformation(expectedKey, expectedMouseAction, expectedModifiers, expectedType) }
            };

            //Act
            hotkeyCommandManager.LoadHotkeyMappings(newMappings);  //Call LoadHotkeyMappings before hotkey is added
            hotkeyCommandManager.AddHotkey(hotkeyId, binding);

            //Assert
            var actualHotkey = hotkeyCommandManager.GetHotkey(hotkeyId);

            if (actualHotkey.Binding is KeyBinding actualKeyBinding)
            {
                Assert.Equal(expectedType, actualHotkey.Binding.GetType());
                Assert.Equal(expectedKey, actualKeyBinding.Key);
                Assert.Equal(expectedModifiers, actualKeyBinding.Modifiers);
            }
            else
            {
                var actualMouseBinding = actualHotkey.Binding as MouseBinding;
                Assert.Equal(expectedType, actualHotkey.Binding.GetType());
                Assert.Equal(expectedMouseAction, actualMouseBinding.MouseAction);
                Assert.Equal(expectedModifiers, (actualMouseBinding.Gesture as MouseGesture).Modifiers);
            }

        }

        public static IEnumerable<object[]> HotkeyMappingRemappedData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[]
                    {
                        "Keybind1",
                        GetInputBinding(Key.A),
                        Key.A,
                        ModifierKeys.None,
                        default(MouseAction),
                        typeof(KeyBinding),
                        0
                    },
                    new object[]
                    {
                        "Keybind2",
                        GetInputBinding(Key.C, ModifierKeys.Control | ModifierKeys.Alt),
                        Key.D,
                        ModifierKeys.Control,
                        default(MouseAction),
                        typeof(KeyBinding),
                        1
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(MouseAction.LeftDoubleClick),
                        default(Key),
                        ModifierKeys.Alt,
                        MouseAction.MiddleDoubleClick,
                        typeof(MouseBinding),
                        1
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(MouseAction.RightClick, ModifierKeys.Shift),
                        Key.F,
                        ModifierKeys.Control | ModifierKeys.Alt,
                        default(MouseAction),
                        typeof(KeyBinding),
                        1
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(MouseAction.RightClick, ModifierKeys.Shift),
                        default(Key),
                        ModifierKeys.Shift,
                        MouseAction.RightClick,
                        typeof(MouseBinding),
                        0
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(HotkeyMappingRemappedData))]
        public void GetRemappedHotkeys_RetrievesOnlyRemappedHotkeys(string hotkeyId, InputBinding binding, Key expectedKey, ModifierKeys expectedModifiers, MouseAction expectedMouseAction, Type expectedType, int expectedCount)
        {
            //Arrange
            var hotkeyCommandManager = GetMockedHotkeyCommandManager();
            hotkeyCommandManager.AddHotkey(hotkeyId, binding);
            var hotkey = hotkeyCommandManager.GetHotkey(hotkeyId);

            //Act
            //update hotkey with new properties.
            hotkey.UpdateHotkey(new HotkeyInformation(expectedKey, expectedMouseAction, expectedModifiers, expectedType));

            //Assert
            Assert.Equal(expectedCount, hotkeyCommandManager.GetRemappedHotkeys().Count);
        }

        
    }
}
