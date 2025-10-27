using System;

namespace UIToolkitDemo
{

    /// <summary>
    /// Public static delegates to manage MainMenu UI changes.
    ///
    ///
    /// Note: these are "events" in the conceptual sense and not the strict C# sense.
    /// </summary>
    public static class MenuEvents
    {

        public static Action MenuScreenShown;
        public static Action MenuScreenHidden;

        public static Action<MenuScreen> CurrentScreenChanged;

        public static Action<string> CurrentViewChanged;

        public static Action PlayButtonClicked;

    }
}
