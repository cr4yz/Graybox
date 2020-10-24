using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox
{
    public enum gb_Bind
    {
        None,
        Append,
        Subtract,
        Select,
        ReduceGrid,
        IncreaseGrid,
        Focus,
        FreeLook,
        FreeLookAccelerate,
        MoveLeft,
        MoveRight,
        MoveForward,
        MoveBack,
        PanCamera,
        Cancel,
        Confirm,
        DuplicateOnDrag
    }

    public class gb_Shortcut
    {
        public string Name;
        public List<KeyCode> Keycodes = new List<KeyCode>();
        public Action Action;
        public bool Triggered;

        public string KeysAsString()
        {
            return string.Join(" + ", Keycodes);
        }

        public bool JustPressed()
        {
            foreach(var key in Keycodes)
            {
                if (!Input.GetKey(key))
                {
                    Triggered = false;
                    return false;
                }
            }
            if (Triggered)
            {
                return false;
            }
            Triggered = true;
            return true;
        }
    }

    public class gb_Binds : gb_Singleton<gb_Binds>
    {

        public static Dictionary<gb_Bind, KeyCode> Binds { get; } = new Dictionary<gb_Bind, KeyCode>()
        {
            { gb_Bind.Append, KeyCode.LeftShift },
            { gb_Bind.Subtract, KeyCode.LeftControl },
            { gb_Bind.Select, KeyCode.Mouse0 },
            { gb_Bind.ReduceGrid, KeyCode.LeftBracket },
            { gb_Bind.IncreaseGrid, KeyCode.RightBracket },
            { gb_Bind.Focus, KeyCode.F },
            { gb_Bind.FreeLook, KeyCode.Mouse1 },
            { gb_Bind.FreeLookAccelerate, KeyCode.LeftShift },
            { gb_Bind.MoveLeft, KeyCode.A },
            { gb_Bind.MoveRight, KeyCode.D },
            { gb_Bind.MoveForward, KeyCode.W },
            { gb_Bind.MoveBack, KeyCode.S },
            { gb_Bind.PanCamera, KeyCode.Mouse2 },
            { gb_Bind.Cancel, KeyCode.Escape },
            { gb_Bind.Confirm, KeyCode.Return },
            { gb_Bind.DuplicateOnDrag, KeyCode.LeftControl },
        };

        public static List<gb_Shortcut> Shortcuts { get; } = new List<gb_Shortcut>();

        public static bool IsDown(gb_Bind input)
        {
            return Binds.ContainsKey(input) && Input.GetKey(Binds[input]);
        }

        public static bool JustDown(gb_Bind input)
        {
            return Binds.ContainsKey(input) && Input.GetKeyDown(Binds[input]);
        }

        public static bool JustUp(gb_Bind input)
        {
            return Binds.ContainsKey(input) && Input.GetKeyUp(Binds[input]);
        }

        public static void RegisterShortcut(string name, Action action, List<KeyCode> keycodes)
        {
            var shortcut = new gb_Shortcut()
            {
                Name = name,
                Action = action,
                Keycodes = keycodes
            };
            Shortcuts.Add(shortcut);
        }

        private void Update()
        {
            foreach(var shortcut in Shortcuts)
            {
                if (shortcut.JustPressed())
                {
                    shortcut.Action?.Invoke();
                }
            }
        }

    }
}

