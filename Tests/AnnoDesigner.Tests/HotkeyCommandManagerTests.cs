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
            var hotkeyCommandManager = new HotkeyCommandManager();
           
            var id = "hotkey";
            var binding = GetInputBinding(Key.A);
            if (addBinding)
            {
                hotkeyCommandManager.AddBinding(id, binding);
            }
            return (hotkeyCommandManager, id, binding);
        }

        // The following code errors:
        /*
            //exception -> do not use Gesture
            //InputBindings.Add(new KeyBinding(_viewModel.ShowMessageCommand, new KeyGesture(Key.D, ModifierKeys.None)));

            //exception -> do not use Gesture
            //InputBindings.Add(new KeyBinding(_viewModel.ShowMessageCommand, Key.D, ModifierKeys.None));

            //exception -> do not use Gesture
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
            var hotkeyCommandManager = new HotkeyCommandManager();

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
        public void AddBinding_NewItemAdded_ShouldSyncToObservableCollection(string id,  InputBinding expectedBinding)
        {
            //Arrange
            var hotkeyCommandManager = new HotkeyCommandManager();

            //Act
            hotkeyCommandManager.AddBinding(id, expectedBinding);
            hotkeyCommandManager.AddBinding(id + "a", expectedBinding);
            hotkeyCommandManager.AddBinding(id + "b", expectedBinding);

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
        public void UpdateBinding_KeyOrMouseUpdated_ShouldSyncToObservableCollection(string id, InputBinding expectedBinding, Key expectedKey, ModifierKeys expectedModifierKeys, MouseAction expectedMouseAction)
        {
            //Arrange
            var hotkeyCommandManager = new HotkeyCommandManager();

            //Act
            hotkeyCommandManager.AddBinding(id, expectedBinding);

            if (expectedBinding is KeyBinding keyBinding)
            {
                //change binding from a KeyBinding to a MouseBinding
                if (expectedMouseAction != default)
                {
                    var hotkey = hotkeyCommandManager.GetBinding(id);
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
                    var hotkey = hotkeyCommandManager.GetBinding(id);
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
        public void RemoveBinding_ShouldSyncToObservableCollection()
        {
            //Arrange
            var (hotkeyCommandManager, id, _) = GetDefaultSetup(true);

            //Act
            hotkeyCommandManager.RemoveHotkey(id);

            //Assert
            Assert.Empty(hotkeyCommandManager.ObservableCollection);
        }

        [Fact]
        public void AddBinding_Duplicate_ShouldThrowArgumentException()
        {
            //Arrange
            var (hotkeyCommandManager, id, binding) = GetDefaultSetup(true);
            //Act and assert
            Assert.Throws<ArgumentException>(() => hotkeyCommandManager.AddBinding(id, binding));
        }

        [Fact]
        public void RemoveBinding_NonExistentBindingId_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            var (hotkeyCommandManager, id, binding) = GetDefaultSetup(true);
            //Act and assert
            Assert.Throws<KeyNotFoundException>(() => hotkeyCommandManager.RemoveHotkey(""));
        }

        [Fact]
        public void GetBinding_NonExistentBindingId_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            var (hotkeyCommandManager, id, binding) = GetDefaultSetup(false);
            //Act and assert
            Assert.Throws<KeyNotFoundException>(() => hotkeyCommandManager.GetBinding(id));
        }
    }
}
