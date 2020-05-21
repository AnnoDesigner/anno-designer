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
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class HotkeyCommandManagerTests
    {
        public HotkeyCommandManagerTests() { }

        private static readonly Action<object> emptyAction = (o) => { };
        private static readonly RelayCommand emptyCommand = new RelayCommand(emptyAction);
        private static readonly PolyGesture emptyGesture = new PolyGesture();

        /// <summary>
        /// Returns a <see cref="HotkeyCommandManager"/> instance, a <see cref="string"/> for an id, and a <see cref="PolyBinding{T}"/>.
        /// </summary>
        /// <param name="addBinding">Set to true to add the binding to the <see cref="HotkeyCommandManager{T}"/> instance</param>
        /// <returns></returns>
        private static (HotkeyCommandManager<PolyBinding<PolyGesture>>, string, PolyBinding<PolyGesture>) GetDefaultSetup(bool addBinding)
        {
            var hotkeyCommandManager = new HotkeyCommandManager<PolyBinding<PolyGesture>>();
            var binding = new PolyBinding<PolyGesture>(emptyCommand, emptyGesture);
            var id = "hotkey";
            if (addBinding)
            {
                hotkeyCommandManager.AddBinding(id, binding);
            }
            return (hotkeyCommandManager, id, binding);
        }
        
        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            //Arrange
            var hotkeyCommandManager = new HotkeyCommandManager<PolyBinding<PolyGesture>>();

            //Act
            var bindings = hotkeyCommandManager.GetBindings();

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
                    new object[] { "Keybind1", new PolyBinding<PolyGesture>(emptyCommand, new PolyGesture(Key.A)) },
                    new object[] { "Keybind2", new PolyBinding<PolyGesture>(emptyCommand, new PolyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Alt)) },
                    new object[] { "Keybind3", new PolyBinding<PolyGesture>(emptyCommand, new PolyGesture(MouseButton.XButton1), "Description") },
                };
            }
        }
        [Theory]
        [MemberData(nameof(NewBindingData))]
        public void AddBinding_NewItemAdded_ShouldSyncToObservableCollection(string id, PolyBinding<PolyGesture> expectedBinding)
        {
            //Arrange
            var hotkeyCommandManager = new HotkeyCommandManager<PolyBinding<PolyGesture>>();

            //Act
            hotkeyCommandManager.AddBinding(id, expectedBinding);
            hotkeyCommandManager.AddBinding(id + "a", expectedBinding);
            hotkeyCommandManager.AddBinding(id + "b", expectedBinding);

            var actualBinding = hotkeyCommandManager.ObservableCollection.First();

            //Assert
            Assert.Equal(3, hotkeyCommandManager.ObservableCollection.Count);
            Assert.Same(expectedBinding, actualBinding);
            Assert.Equal(expectedBinding.Description, actualBinding.Description);
            Assert.Equal(expectedBinding.Gesture.Key, actualBinding.Gesture.Key);
            Assert.Equal(expectedBinding.Gesture.ModifierKeys, actualBinding.Gesture.ModifierKeys);
            Assert.Equal(expectedBinding.Gesture.MouseButton, actualBinding.Gesture.MouseButton);
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
                        new PolyBinding<PolyGesture>(emptyCommand, new PolyGesture(Key.A)), 
                        Key.B,
                        ModifierKeys.None, 
                        default(MouseButton) 
                    },
                    new object[] 
                    { 
                        "Keybind2", 
                        new PolyBinding<PolyGesture>(emptyCommand, new PolyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Alt)), 
                        Key.D, 
                        ModifierKeys.Control, 
                        default(MouseButton)
                    },
                    new object[] 
                    { 
                        "Keybind3", 
                        new PolyBinding<PolyGesture>(emptyCommand, new PolyGesture(MouseButton.XButton1), "Description"), 
                        default(Key), 
                        default(ModifierKeys), 
                        MouseButton.XButton2,
                        "Updated Description"
                    },
                    new object[]
                    {
                        "Keybind3",
                        new PolyBinding<PolyGesture>(emptyCommand, new PolyGesture(MouseButton.XButton1), "Change from mouse to key gesture"),
                        Key.F,
                        ModifierKeys.Control | ModifierKeys.Alt,
                        default(MouseButton)
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(UpdateBindingData))]
        public void UpdateBinding_KeyOrMouseUpdated_ShouldSyncToObservableCollection(string id, PolyBinding<PolyGesture> binding, Key expectedKey, ModifierKeys expectedModifierKeys, MouseButton expectedMouseButton, string expectedDescription = null)
        {
            //Arrange
            var hotkeyCommandManager = new HotkeyCommandManager<PolyBinding<PolyGesture>>();

            //Act
            hotkeyCommandManager.AddBinding(id, binding);
            var bindingToUpdate = hotkeyCommandManager.GetBinding(id);
            if (expectedMouseButton == default)
            {
                bindingToUpdate.Gesture.SetKey(expectedKey, expectedModifierKeys);
            }
            else
            {
                bindingToUpdate.Gesture.SetMouseButton(expectedMouseButton);
            }
            bindingToUpdate.Description = expectedDescription;

            var actualBinding = hotkeyCommandManager.ObservableCollection.First();
            //Assert
            Assert.Same(binding, bindingToUpdate);
            Assert.Same(binding, actualBinding);
            Assert.Equal(expectedKey, actualBinding.Gesture.Key);
            Assert.Equal(expectedModifierKeys, actualBinding.Gesture.ModifierKeys);
            Assert.Equal(expectedMouseButton, actualBinding.Gesture.MouseButton);
            Assert.Equal(expectedDescription, actualBinding.Description);
        }
        
        [Fact]
        public void RemoveBinding_ShouldSyncToObservableCollection()
        {
            //Arrange
            var (hotkeyCommandManager, id, _) = GetDefaultSetup(true);

            //Act
            hotkeyCommandManager.RemoveBinding(id);

            //Assert
            Assert.Empty(hotkeyCommandManager.ObservableCollection);
        }

        public void AddBinding_Duplicate_ShouldThrowException()
        {

        }

        public void RemoveBinding_AlreadyEmpty_ShouldThrowException()
        {

        }
    }
}
