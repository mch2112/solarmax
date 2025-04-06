using System.Collections.Generic;
using System.Windows.Forms;

namespace SolarMax;

internal enum CommandCode { None, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, Tab, MouseHorizontal, MouseVertical, MouseWheel, MouseClick, LeftArrow, RightArrow, UpArrow, DownArrow, BackslashPipe, Space, Insert, CommaLeftAngle, PeriodRightAngle, QuestionSlash, Escape, Backspace, PlusEquals, HyphenUnderscore, Enter, 
                            Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, PageUp, PageDown, Home, End, Pause, TildeBackTick }
internal class QCommand
{
    public CommandCode CommandCode { get; private set; }
    public string String { get; private set; }
    public double Data { get; private set; }
    public QPoint Point { get; private set; }
    public bool Shift { get; private set; }
    public bool Control { get; private set; }
    public bool Alt { get; private set; }

    static QCommand()
    {
        CommandCode kc = CommandCode.A;
        for (Keys k = Keys.A; k <= Keys.Z; k++)
            keyDictionary.Add(k, kc++);
        kc = CommandCode.F1;
        for (Keys k = Keys.F1; k <= Keys.F12; k++)
            keyDictionary.Add(k, kc++);
        kc = CommandCode.Zero;
        for (Keys k = Keys.D0; k <= Keys.D9; k++)
            keyDictionary.Add(k, kc++);
        kc = CommandCode.Zero;
        for (Keys k = Keys.NumPad0; k <= Keys.NumPad9; k++)
            keyDictionary.Add(k, kc++);
    }
    #region KeyDictionary

    private static readonly Dictionary<Keys, CommandCode> keyDictionary = new() { { Keys.Left,    CommandCode.LeftArrow },
                                                                                  { Keys.Right,   CommandCode.RightArrow },
                                                                                  { Keys.Up,      CommandCode.UpArrow },
                                                                                  { Keys.Down,    CommandCode.DownArrow },
                                                                                  { Keys.OemPipe, CommandCode.BackslashPipe },
                                                                                  { Keys.Escape,  CommandCode.Escape },
                                                                                  { Keys.Back,    CommandCode.Backspace },
                                                                                  { Keys.Oemplus, CommandCode.PlusEquals },
                                                                                  { Keys.Enter,   CommandCode.Enter },
                                                                                  { Keys.PageUp,  CommandCode.PageUp },
                                                                                  { Keys.PageDown, CommandCode.PageDown },
                                                                                  { Keys.Home,    CommandCode.Home },
                                                                                  { Keys.End,     CommandCode.End },
                                                                                  { Keys.Pause,   CommandCode.Pause },
                                                                                  { Keys.Space,   CommandCode.Space },
                                                                                  { Keys.Oemtilde, CommandCode.TildeBackTick },
                                                                                  { Keys.Insert,  CommandCode.Insert },
                                                                                  { Keys.Oemcomma, CommandCode.CommaLeftAngle },
                                                                                  { Keys.OemPeriod, CommandCode.PeriodRightAngle },
                                                                                  { Keys.OemQuestion, CommandCode.QuestionSlash },
                                                                                  { Keys.OemMinus, CommandCode.HyphenUnderscore },
                                                                                  { Keys.Tab,      CommandCode.Tab }
    };
    #endregion
    public QCommand(Keys Key, bool Shift, bool Control, bool Alt, string String = "", double Data = 0)
    {
        this.Shift = Shift;
        this.Control = Control;
        this.Alt = Alt;
        this.String = String;
        this.Data = Data;
        this.Point = QPoint.Empty;

        if (keyDictionary.TryGetValue(Key, out CommandCode kc))
            this.CommandCode = kc;
        else
            this.CommandCode = CommandCode.None;

        if (this.String.Length == 0)
        {
            if (this.CommandCode >= CommandCode.A && this.CommandCode <= CommandCode.Z)
            {
                this.String = ((char)('A' + this.CommandCode - CommandCode.A)).ToString();
            }
            else if (this.CommandCode >= CommandCode.Zero && this.CommandCode <= CommandCode.Nine)
            {
                this.String = ((char)('0' + this.CommandCode - CommandCode.Zero)).ToString();
            }
        }
    }
    public QCommand(CommandCode Code, bool Shift, bool Control, bool Alt, double Data = 0, string String = "")
    {
        this.Shift = Shift;
        this.Control = Control;
        this.Alt = Alt;
        this.String = String;
        this.Data = Data;
        this.CommandCode = Code;
        this.Point = QPoint.Empty;
    }
    public QCommand(CommandCode Code, bool Shift, bool Control, bool Alt, QPoint Point, double Data = 0, string String = "")
    {
        this.Shift = Shift;
        this.Control = Control;
        this.Alt = Alt;
        this.String = String;
        this.Data = Data;
        this.CommandCode = Code;
        this.Point = Point;
    }
}
