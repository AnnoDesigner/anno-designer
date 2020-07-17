using System;
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
        private static (HotkeyCommandManager, string, InputBinding) GetDefaultSetup(bool addBinding)
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
            return new HotkeyCommandManager(Mock.Of<ILocalizationHelper>());
        }

        private static InputBinding GetInputBinding(Key key)
        {
            return GetInputBinding(key, ModifierKeys.None);
        }
        private static InputBinding GetInputBinding(ExtendedMouseAction mouseAction)
        {
            return GetInputBinding(mouseAction, ModifierKeys.None);
        }

        private static InputBinding GetInputBinding(ExtendedMouseAction mouseAction, ModifierKeys modifierKeys)
        {
            return new InputBinding(emptyCommand, new PolyGesture(mouseAction, modifierKeys));
        }

        private static InputBinding GetInputBinding(Key key, ModifierKeys modifierKeys)
        {
            return new InputBinding(emptyCommand, new PolyGesture(key, modifierKeys));
        }

        private void AssertPolyGestureMatches(PolyGesture expectedGesture, PolyGesture actualGesture)
        {
            Assert.Equal(expectedGesture.Key, actualGesture.Key);
            Assert.Equal(expectedGesture.MouseAction, actualGesture.MouseAction);
            Assert.Equal(expectedGesture.ModifierKeys, actualGesture.ModifierKeys);
            Assert.Equal(expectedGesture.Type, actualGesture.Type);
        }

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            //Arrange
            var hotkeyCommandManager = GetMockedHotkeyCommandManager();

            //Act
            var hotkeys = hotkeyCommandManager.GetHotkeys();

            //Assert
            Assert.NotNull(hotkeyCommandManager.ObservableCollection);
            Assert.NotNull(hotkeys);
            Assert.Empty(hotkeys);
        }

        public static IEnumerable<object[]> NewHotkeyData
        {
            get
            {
                return new List<object[]>()
                {
                    new object[] { "Keybind1", GetInputBinding(Key.A) },
                    new object[] { "Keybind2", GetInputBinding(Key.B, ModifierKeys.Alt | ModifierKeys.Control) },
                    new object[] { "Keybind3", GetInputBinding(Key.C, ModifierKeys.Shift) },
                    new object[] { "Keybind4", GetInputBinding(ExtendedMouseAction.LeftClick) },
                    new object[] { "Keybind5", GetInputBinding(ExtendedMouseAction.RightDoubleClick, ModifierKeys.Control) }
                };
            }
        }
        [Theory]
        [MemberData(nameof(NewHotkeyData))]
        //Modifiers
        public void AddHotkey_NewItemAdded_ShouldSyncToObservableCollection(string id, InputBinding expectedBinding)
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

            var actualGesture = hotkey.Binding.Gesture as PolyGesture;
            var expectedGesture = expectedBinding.Gesture as PolyGesture;

            AssertPolyGestureMatches(expectedGesture, actualGesture);

        }

        public static IEnumerable<object[]> UpdateHotkeyData
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
                        default(ExtendedMouseAction)
                    },
                    new object[]
                    {
                        "Keybind2",
                        GetInputBinding(Key.C, ModifierKeys.Control | ModifierKeys.Alt),
                        Key.D,
                        ModifierKeys.Control,
                        default(ExtendedMouseAction)
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(ExtendedMouseAction.LeftDoubleClick),
                        default(Key),
                        ModifierKeys.Alt,
                        ExtendedMouseAction.MiddleDoubleClick
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(ExtendedMouseAction.RightClick, ModifierKeys.Shift),
                        Key.F,
                        ModifierKeys.Control | ModifierKeys.Alt,
                        default(MouseAction)
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(UpdateHotkeyData))]
        public void UpdateHotkey_KeyOrMouseUpdated_ShouldSyncToObservableCollection(string id, InputBinding expectedBinding, Key expectedKey, ModifierKeys expectedModifierKeys, ExtendedMouseAction expectedMouseAction)
        {
            //Arrange
            var hotkeyCommandManager = GetMockedHotkeyCommandManager();

            //Act
            hotkeyCommandManager.AddHotkey(id, expectedBinding);

            var gesture = expectedBinding.Gesture as PolyGesture;
            gesture.Key = expectedKey;
            gesture.ModifierKeys = expectedModifierKeys;
            gesture.MouseAction = expectedMouseAction;

            if (expectedMouseAction != default)
            {
                gesture.Type = GestureType.MouseGesture;
            }
            else
            {
                gesture.Type = GestureType.KeyGesture;
            }

            //if (expectedBinding is KeyBinding keyBinding)
            //{
            //    //change binding from a KeyBinding to a MouseBinding
            //    if (expectedMouseAction != default)
            //    {
            //        var hotkey = hotkeyCommandManager.GetHotkey(id);
            //        hotkey.Binding = new MouseBinding(emptyCommand, new MouseGesture(expectedMouseAction, expectedModifierKeys));
            //        expectedBinding = hotkey.Binding;
            //    }
            //    else
            //    {
            //        keyBinding.Key = expectedKey;
            //        keyBinding.Modifiers = expectedModifierKeys;
            //    }
            //}
            //else
            //{
            //    var mouseBinding = expectedBinding as MouseBinding;

            //    //Change binding from a MouseBinding to a KeyBinding
            //    if (expectedKey != default)
            //    {
            //        var hotkey = hotkeyCommandManager.GetHotkey(id);
            //        hotkey.Binding = new KeyBinding
            //        {
            //            Command = emptyCommand,
            //            Key = expectedKey,
            //            Modifiers = expectedModifierKeys
            //        };
            //        expectedBinding = hotkey.Binding;
            //    }
            //    else
            //    {
            //        //Can't set the Modifiers property without creating a new MouseGesture.
            //        mouseBinding.Gesture = new MouseGesture(expectedMouseAction, expectedModifierKeys);
            //    }
            //}

            var actualBinding = hotkeyCommandManager.ObservableCollection.First().Binding;

            //Assert

            Assert.Same(expectedBinding, actualBinding);
            var actualGesture = actualBinding.Gesture as PolyGesture;
            var expectedGesture = expectedBinding.Gesture as PolyGesture;
            AssertPolyGestureMatches(expectedGesture, actualGesture);
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
        public void RemoveHotkey_NonExistentHotkeyId_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            var (hotkeyCommandManager, id, binding) = GetDefaultSetup(true);
            //Act and assert
            Assert.Throws<KeyNotFoundException>(() => hotkeyCommandManager.RemoveHotkey(""));
        }

        [Fact]
        public void GetHotkey_NonExistentHotkeyId_ShouldThrowKeyNotFoundException()
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
                //use an invalid value for the GestureType enum
                { "myHotkeyInfo", new HotkeyInformation(Key.A, default, ModifierKeys.None, (GestureType)int.MaxValue) }
            };
            //Act
            hotkeyCommandMananger.LoadHotkeyMappings(newMappings);
            hotkeyCommandMananger.AddHotkey("myHotkeyInfo", new InputBinding(emptyCommand, new PolyGesture(Key.C, ModifierKeys.Control)));
            //Assert
            var gesture = hotkeyCommandMananger.GetHotkey("myHotkeyInfo").Binding.Gesture as PolyGesture;
            Assert.Equal(Key.C, gesture.Key);
            Assert.Equal(ModifierKeys.Control, gesture.ModifierKeys);
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
                        GestureType.KeyGesture
                    },
                    new object[]
                    {
                        "Keybind2",
                        GetInputBinding(Key.C, ModifierKeys.Control | ModifierKeys.Alt),
                        Key.D,
                        ModifierKeys.Control,
                        default(MouseAction),
                        GestureType.KeyGesture
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(ExtendedMouseAction.LeftDoubleClick),
                        default(Key),
                        ModifierKeys.Alt,
                        ExtendedMouseAction.MiddleDoubleClick,
                        GestureType.MouseGesture
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(ExtendedMouseAction.RightClick, ModifierKeys.Shift),
                        Key.F,
                        ModifierKeys.Control | ModifierKeys.Alt,
                        default(MouseAction),
                        GestureType.KeyGesture
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(ExtendedMouseAction.RightClick, ModifierKeys.Shift),
                        default(Key),
                        ModifierKeys.Shift,
                        ExtendedMouseAction.RightClick,
                        GestureType.MouseGesture
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(HotkeyMappingLoadData))]
        public void LoadHotkeyMappings_CalledAfterHotkeyIsAdded_LoadsNewHotkeyMappings(string hotkeyId, InputBinding binding, Key expectedKey, ModifierKeys expectedModifiers, ExtendedMouseAction expectedMouseAction, GestureType expectedType)
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
            var actualGesture = actualHotkey.Binding.Gesture as PolyGesture;

            Assert.Equal(expectedKey, actualGesture.Key);
            Assert.Equal(expectedMouseAction, actualGesture.MouseAction);
            Assert.Equal(expectedModifiers, actualGesture.ModifierKeys);
            Assert.Equal(expectedType, actualGesture.Type);
        }

        [Theory]
        [MemberData(nameof(HotkeyMappingLoadData))]
        public void LoadHotkeyMappings_CalledBeforeHotkeyIsAdded_LoadsNewHotkeyMappings(string hotkeyId, InputBinding binding, Key expectedKey, ModifierKeys expectedModifiers, ExtendedMouseAction expectedMouseAction, GestureType expectedType)
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
            var actualGesture = actualHotkey.Binding.Gesture as PolyGesture;

            Assert.Equal(expectedKey, actualGesture.Key);
            Assert.Equal(expectedMouseAction, actualGesture.MouseAction);
            Assert.Equal(expectedModifiers, actualGesture.ModifierKeys);
            Assert.Equal(expectedType, actualGesture.Type);

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
                        GestureType.KeyGesture,
                        0
                    },
                    new object[]
                    {
                        "Keybind2",
                        GetInputBinding(Key.C, ModifierKeys.Control | ModifierKeys.Alt),
                        Key.D,
                        ModifierKeys.Control,
                        default(MouseAction),
                        GestureType.KeyGesture,
                        1
                    },
                    new object[]
                    {
                        "Keybind3",
                        GetInputBinding(ExtendedMouseAction.LeftDoubleClick),
                        default(Key),
                        ModifierKeys.Alt,
                        ExtendedMouseAction.MiddleDoubleClick,
                        GestureType.MouseGesture,
                        1
                    },
                    new object[]
                    {
                        "Keybind4",
                        GetInputBinding(ExtendedMouseAction.RightClick, ModifierKeys.Shift),
                        Key.F,
                        ModifierKeys.Control | ModifierKeys.Alt,
                        default(MouseAction),
                        GestureType.KeyGesture,
                        1
                    },
                    new object[]
                    {
                        "Keybind5",
                        GetInputBinding(ExtendedMouseAction.RightClick, ModifierKeys.Shift),
                        default(Key),
                        ModifierKeys.Shift,
                        ExtendedMouseAction.RightClick,
                        GestureType.MouseGesture,
                        0
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(HotkeyMappingRemappedData))]
        public void GetRemappedHotkeys_RetrievesOnlyRemappedHotkeys(string hotkeyId, InputBinding binding, Key expectedKey, ModifierKeys expectedModifiers, ExtendedMouseAction expectedMouseAction, GestureType expectedType, int expectedCount)
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
