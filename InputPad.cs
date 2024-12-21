// WindowsPhoneSpeedyBlupi, Version=1.0.0.5, Culture=neutral, PublicKeyToken=6db12cd62dbec439
// WindowsPhoneSpeedyBlupi.InputPad
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using static WindowsPhoneSpeedyBlupi.Def;

namespace WindowsPhoneSpeedyBlupi
{
    public class InputPad
    {
        private static readonly int padSize = 140;

        private readonly Game1 game1;

        private readonly Decor decor;

        private readonly Pixmap pixmap;

        private readonly Sound sound;

        private readonly GameData gameData;

        private readonly List<Def.ButtonGlyph> pressedGlyphs;

        private readonly Accelerometer accelSensor;

        private readonly Slider accelSlider;

        private bool padPressed;

        private bool showCheatMenu;

        private TinyPoint padTouchPos;

        private Def.ButtonGlyph lastButtonDown;

        private Def.ButtonGlyph buttonPressed;

        private int touchCount;

        private bool accelStarted;

        private bool accelActive;

        private double accelSpeedX;

        private bool accelLastState;

        private bool accelWaitZero;

        private int mission;

        public Def.Phase Phase { get; set; }

        public int SelectedGamer { get; set; }

        public TinyPoint PixmapOrigin { get; set; }

        public int TotalTouch
        {
            get
            {
                return touchCount;
            }
        }

        public Def.ButtonGlyph ButtonPressed
        {
            get
            {
                Def.ButtonGlyph result = buttonPressed;
                buttonPressed = Def.ButtonGlyph.None;
                return result;
            }
        }

        public bool ShowCheatMenu
        {
            get
            {
                return showCheatMenu;
            }
            set
            {
                showCheatMenu = value;
            }
        }

        private IEnumerable<Def.ButtonGlyph> ButtonGlyphs
        {
            get
            {
                switch (Phase)
                {
                    case Def.Phase.Init:
                        yield return Def.ButtonGlyph.InitGamerA;
                        yield return Def.ButtonGlyph.InitGamerB;
                        yield return Def.ButtonGlyph.InitGamerC;
                        yield return Def.ButtonGlyph.InitSetup;
                        yield return Def.ButtonGlyph.InitPlay;
                        if (game1.IsTrialMode)
                        {
                            yield return Def.ButtonGlyph.InitBuy;
                        }
                        if (game1.IsRankingMode)
                        {
                            yield return Def.ButtonGlyph.InitRanking;
                        }
                        break;
                    case Def.Phase.Play:
                        yield return Def.ButtonGlyph.PlayPause;
                        yield return Def.ButtonGlyph.PlayAction;
                        yield return Def.ButtonGlyph.PlayJump;
                        if (accelStarted)
                        {
                            yield return Def.ButtonGlyph.PlayDown;
                        }
                        yield return Def.ButtonGlyph.Cheat11;
                        yield return Def.ButtonGlyph.Cheat12;
                        yield return Def.ButtonGlyph.Cheat21;
                        yield return Def.ButtonGlyph.Cheat22;
                        yield return Def.ButtonGlyph.Cheat31;
                        yield return Def.ButtonGlyph.Cheat32;
                        break;
                    case Def.Phase.Pause:
                        yield return Def.ButtonGlyph.PauseMenu;
                        if (mission != 1)
                        {
                            yield return Def.ButtonGlyph.PauseBack;
                        }
                        yield return Def.ButtonGlyph.PauseSetup;
                        if (mission != 1 && mission % 10 != 0)
                        {
                            yield return Def.ButtonGlyph.PauseRestart;
                        }
                        yield return Def.ButtonGlyph.PauseContinue;
                        break;
                    case Def.Phase.Resume:
                        yield return Def.ButtonGlyph.ResumeMenu;
                        yield return Def.ButtonGlyph.ResumeContinue;
                        break;
                    case Def.Phase.Lost:
                    case Def.Phase.Win:
                        yield return Def.ButtonGlyph.WinLostReturn;
                        break;
                    case Def.Phase.Trial:
                        yield return Def.ButtonGlyph.TrialBuy;
                        yield return Def.ButtonGlyph.TrialCancel;
                        break;
                    case Def.Phase.MainSetup:
                        yield return Def.ButtonGlyph.SetupSounds;
                        yield return Def.ButtonGlyph.SetupJump;
                        yield return Def.ButtonGlyph.SetupZoom;
                        yield return Def.ButtonGlyph.SetupAccel;
                        yield return Def.ButtonGlyph.SetupReset;
                        yield return Def.ButtonGlyph.SetupReturn;
                        break;
                    case Def.Phase.PlaySetup:
                        yield return Def.ButtonGlyph.SetupSounds;
                        yield return Def.ButtonGlyph.SetupJump;
                        yield return Def.ButtonGlyph.SetupZoom;
                        yield return Def.ButtonGlyph.SetupAccel;
                        yield return Def.ButtonGlyph.SetupReturn;
                        break;
                    case Def.Phase.Ranking:
                        yield return Def.ButtonGlyph.RankingContinue;
                        break;
                }
                if (showCheatMenu)
                {
                    yield return Def.ButtonGlyph.Cheat1;
                    yield return Def.ButtonGlyph.Cheat2;
                    yield return Def.ButtonGlyph.Cheat3;
                    yield return Def.ButtonGlyph.Cheat4;
                    yield return Def.ButtonGlyph.Cheat5;
                    yield return Def.ButtonGlyph.Cheat6;
                    yield return Def.ButtonGlyph.Cheat7;
                    yield return Def.ButtonGlyph.Cheat8;
                    yield return Def.ButtonGlyph.Cheat9;
                }
            }
        }
        /// <summary>
        /// Returns the point of the cente of the pad on the screen.
        /// </summary>
        private TinyPoint PadCenter
        {
            get
            {
                TinyRect drawBounds = pixmap.DrawBounds;
                int x = gameData.JumpRight ? 100 : drawBounds.Width - 100;
                return new TinyPoint(x, drawBounds.Height - 100);
            }
        }

        public InputPad(Game1 game1, Decor decor, Pixmap pixmap, Sound sound, GameData gameData)
        {
            //IL_0037: Unknown result type (might be due to invalid IL or missing references)
            //IL_0041: Expected O, but got Unknown
            this.game1 = game1;
            this.decor = decor;
            this.pixmap = pixmap;
            this.sound = sound;
            this.gameData = gameData;
            pressedGlyphs = new List<Def.ButtonGlyph>();
            accelSensor = new Accelerometer();
            ((SensorBase<AccelerometerReading>)(object)accelSensor).CurrentValueChanged += HandleAccelSensorCurrentValueChanged;
            accelSlider = new Slider(new TinyPoint(320, 400), this.gameData.AccelSensitivity);
            lastButtonDown = Def.ButtonGlyph.None;
            buttonPressed = Def.ButtonGlyph.None;
        }

        public void StartMission(int mission)
        {
            this.mission = mission;
            accelWaitZero = true;
        }

        public void Update()
        {
            pressedGlyphs.Clear();
            if (accelActive != gameData.AccelActive)
            {
                accelActive = gameData.AccelActive;
                if (accelActive)
                {
                    StartAccel();
                }
                else
                {
                    StopAccel();
                }
            }
            double horizontalChange = 0.0;
            double verticalChange = 0.0;
            int num3 = 0;
            padPressed = false;
            Def.ButtonGlyph buttonGlyph = Def.ButtonGlyph.None;
            TouchCollection touches = TouchPanel.GetState();
            touchCount = touches.Count;
            List<TinyPoint> touchesOrClicks = new List<TinyPoint>();
            foreach (TouchLocation item in touches)
            {
                if (item.State == TouchLocationState.Pressed || item.State == TouchLocationState.Moved)
                {
                    TinyPoint touchPress = new TinyPoint((int)item.Position.X, (int)item.Position.Y);
                    touchesOrClicks.Add(touchPress);
                }
            }

            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                touchCount++;
                TinyPoint mouseClick = new TinyPoint(mouseState.X, mouseState.Y);
                touchesOrClicks.Add(mouseClick);
            }
            
            float screenWidth = game1.getGraphics().GraphicsDevice.Viewport.Width;
            float screenHeight = game1.getGraphics().GraphicsDevice.Viewport.Height;
            float screenRatio = screenWidth / screenHeight;

            if (Def.PLATFORM == Platform.Android &&screenRatio > 1.3333333333333333)
            {
                for (int i = 0; i < touchesOrClicks.Count; i++)
                {

                    var touchOrClick = touchesOrClicks[i];
                    if (touchOrClick.X == -1) continue;

                    float originalX = touchOrClick.X;
                    float originalY = touchOrClick.Y;

                    float widthHeightRatio = screenWidth / screenHeight;
                    float heightRatio = 480 / screenHeight;
                    float widthRatio = 640 / screenWidth;
                    if(Def.DETAILED_DEBUGGING)
                    {
                    Debug.WriteLine("-----");
                    Debug.WriteLine("originalX=" + originalX);
                    Debug.WriteLine("originalY=" + originalY);
                    Debug.WriteLine("heightRatio=" + heightRatio);
                    Debug.WriteLine("widthRatio=" + widthRatio);
                    Debug.WriteLine("widthHeightRatio=" + widthHeightRatio);
                    }
                    if (screenHeight> 480) {
                    touchOrClick.X = (int)(originalX * heightRatio);
                    touchOrClick.Y = (int)(originalY * heightRatio);
                    touchesOrClicks[i] = touchOrClick;
                    }

                    if(Def.DETAILED_DEBUGGING) Debug.WriteLine("new X" + touchOrClick.X);
                    if(Def.DETAILED_DEBUGGING) Debug.WriteLine("new Y" + touchOrClick.Y);
                }
            }

            KeyboardState newKeyboardState = Keyboard.GetState();
            Keys[] keysToBeChecked = new Keys[6] { Keys.LeftControl, Keys.Up, Keys.Right, Keys.Down, Keys.Left, Keys.Space };
            foreach(Keys keys in keysToBeChecked) {
                if (newKeyboardState.IsKeyDown(keys)) touchesOrClicks.Add(new TinyPoint(-1, (int)keys));
            }
            if (newKeyboardState.IsKeyDown(Keys.F11))
            {
                game1.ToggleFullScreen ();
                Debug.WriteLine("F11 was pressed.");
            }

            Boolean keyPressedUp = false;
            Boolean keyPressedDown = false;
            Boolean keyPressedLeft = false;
            Boolean keyPressedRight = false;
            foreach (TinyPoint touchOrClick in touchesOrClicks)
            {
                Boolean keyboardPressed = false;
                if (touchOrClick.X == -1)
                {
                    keyboardPressed = true;
                }
                Keys keyPressed = keyboardPressed ? (Keys)touchOrClick.Y : Keys.None;
                keyPressedUp = keyPressed == Keys.Up ? true : keyPressedUp;
                keyPressedDown = keyPressed == Keys.Down ? true : keyPressedDown;
                keyPressedLeft = keyPressed == Keys.Left ? true : keyPressedLeft;
                keyPressedRight = keyPressed == Keys.Right ? true : keyPressedRight;

                {
                    TinyPoint tinyPoint2 = keyboardPressed ? new TinyPoint(1, 1) : touchOrClick;
                    if (!accelStarted && Misc.IsInside(GetPadBounds(PadCenter, padSize), tinyPoint2))
                    {
                        padPressed = true;
                        padTouchPos = tinyPoint2;
                    }
                    if (keyPressed == Keys.Up || keyPressed == Keys.Right || keyPressed == Keys.Down || keyPressed == Keys.Left)
                    {
                        padPressed = true;
                    }
                    Debug.WriteLine("padPressed=" + padPressed);
                    Def.ButtonGlyph buttonGlyph2 = ButtonDetect(tinyPoint2);
                    Debug.WriteLine("buttonGlyph2 =" + buttonGlyph2);
                    if (buttonGlyph2 != 0)
                    {
                        pressedGlyphs.Add(buttonGlyph2);
                    }
                    if (keyboardPressed)
                    {
                        switch (keyPressed)
                        {
                            case Keys.LeftControl: buttonGlyph2 = Def.ButtonGlyph.PlayJump; pressedGlyphs.Add(buttonGlyph2); break;
                            case Keys.Space: buttonGlyph2 = Def.ButtonGlyph.PlayAction; pressedGlyphs.Add(buttonGlyph2); break;
                        }
                    }

                    if ((Phase == Def.Phase.MainSetup || Phase == Def.Phase.PlaySetup) && accelSlider.Move(tinyPoint2))
                    {
                        gameData.AccelSensitivity = accelSlider.Value;
                    }
                    switch (buttonGlyph2)
                    {
                        case Def.ButtonGlyph.PlayJump:
                            Debug.WriteLine("Jumping detected");
                            accelWaitZero = false;
                            num3 |= 1;
                            break;
                        case Def.ButtonGlyph.PlayDown:
                            accelWaitZero = false;
                            num3 |= 4;
                            break;
                        case Def.ButtonGlyph.InitGamerA:
                        case Def.ButtonGlyph.InitGamerB:
                        case Def.ButtonGlyph.InitGamerC:
                        case Def.ButtonGlyph.InitSetup:
                        case Def.ButtonGlyph.InitPlay:
                        case Def.ButtonGlyph.InitBuy:
                        case Def.ButtonGlyph.InitRanking:
                        case Def.ButtonGlyph.WinLostReturn:
                        case Def.ButtonGlyph.TrialBuy:
                        case Def.ButtonGlyph.TrialCancel:
                        case Def.ButtonGlyph.SetupSounds:
                        case Def.ButtonGlyph.SetupJump:
                        case Def.ButtonGlyph.SetupZoom:
                        case Def.ButtonGlyph.SetupAccel:
                        case Def.ButtonGlyph.SetupReset:
                        case Def.ButtonGlyph.SetupReturn:
                        case Def.ButtonGlyph.PauseMenu:
                        case Def.ButtonGlyph.PauseBack:
                        case Def.ButtonGlyph.PauseSetup:
                        case Def.ButtonGlyph.PauseRestart:
                        case Def.ButtonGlyph.PauseContinue:
                        case Def.ButtonGlyph.PlayPause:
                        case Def.ButtonGlyph.PlayAction:
                        case Def.ButtonGlyph.ResumeMenu:
                        case Def.ButtonGlyph.ResumeContinue:
                        case Def.ButtonGlyph.RankingContinue:
                        case Def.ButtonGlyph.Cheat11:
                        case Def.ButtonGlyph.Cheat12:
                        case Def.ButtonGlyph.Cheat21:
                        case Def.ButtonGlyph.Cheat22:
                        case Def.ButtonGlyph.Cheat31:
                        case Def.ButtonGlyph.Cheat32:
                        case Def.ButtonGlyph.Cheat1:
                        case Def.ButtonGlyph.Cheat2:
                        case Def.ButtonGlyph.Cheat3:
                        case Def.ButtonGlyph.Cheat4:
                        case Def.ButtonGlyph.Cheat5:
                        case Def.ButtonGlyph.Cheat6:
                        case Def.ButtonGlyph.Cheat7:
                        case Def.ButtonGlyph.Cheat8:
                        case Def.ButtonGlyph.Cheat9:
                            accelWaitZero = false;
                            buttonGlyph = buttonGlyph2;
                            showCheatMenu = false;
                            break;
                    }
                }
            }
            if (buttonGlyph != 0 && buttonGlyph != Def.ButtonGlyph.PlayAction && buttonGlyph != Def.ButtonGlyph.Cheat11 && buttonGlyph != Def.ButtonGlyph.Cheat12 && buttonGlyph != Def.ButtonGlyph.Cheat21 && buttonGlyph != Def.ButtonGlyph.Cheat22 && buttonGlyph != Def.ButtonGlyph.Cheat31 && buttonGlyph != Def.ButtonGlyph.Cheat32 && lastButtonDown == Def.ButtonGlyph.None)
            {
                TinyPoint tinyPoint3 = default(TinyPoint);
                tinyPoint3.X = 320;
                tinyPoint3.Y = 240;
                TinyPoint pos = tinyPoint3;
                sound.PlayImage(0, pos);
            }
            if (buttonGlyph == Def.ButtonGlyph.None && lastButtonDown != 0)
            {
                buttonPressed = lastButtonDown;
            }
            lastButtonDown = buttonGlyph;
            if (padPressed)
            {
                Debug.WriteLine("PadCenter.X=" + PadCenter.X);
                Debug.WriteLine("PadCenter.Y=" + PadCenter.Y);
                Debug.WriteLine("padTouchPos.X=" + padTouchPos.X);
                Debug.WriteLine("padTouchPos.Y=" + padTouchPos.Y);
                Debug.WriteLine("keyPressedUp=" + keyPressedUp);
                Debug.WriteLine("keyPressedDown=" + keyPressedDown);
                Debug.WriteLine("keyPressedLeft=" + keyPressedLeft);
                Debug.WriteLine(" keyPressedRight=" + keyPressedRight);
                {
                    if (keyPressedUp)
                    {
                        padTouchPos.Y = PadCenter.Y - 30;
                        padTouchPos.X = PadCenter.X;
                        if (keyPressedLeft) padTouchPos.X = PadCenter.X - 30;
                        if (keyPressedRight) padTouchPos.X = PadCenter.X + 30;
                    }
                    if (keyPressedDown) { 
                        padTouchPos.Y = PadCenter.Y + 30;
                        padTouchPos.X = PadCenter.X;
                        if (keyPressedLeft) padTouchPos.X = PadCenter.X - 30;
                        if (keyPressedRight) padTouchPos.X = PadCenter.X + 30;
                    }
                    if (keyPressedLeft) { 
                        padTouchPos.X = PadCenter.X - 30;
                        padTouchPos.Y = PadCenter.Y;
                        if (keyPressedUp) padTouchPos.Y = PadCenter.Y - 30;
                        if (keyPressedDown) padTouchPos.Y = PadCenter.Y + 30;
                    }
                    if (keyPressedRight) { 
                        padTouchPos.X = PadCenter.X + 30;
                        padTouchPos.Y = PadCenter.Y;
                        if (keyPressedUp) padTouchPos.Y = PadCenter.Y - 30;
                        if (keyPressedDown) padTouchPos.Y = PadCenter.Y + 30;
                    }
                }
                double horizontalPosition = padTouchPos.X - PadCenter.X;
                double verticalPosition = padTouchPos.Y - PadCenter.Y;

                if (horizontalPosition > 20.0)
                {
                    horizontalChange += 1.0;
                    Debug.WriteLine(" horizontalChange += 1.0;");
                }
                if (horizontalPosition < -20.0)
                {
                    horizontalChange -= 1.0;
                    Debug.WriteLine(" horizontalChange -= 1.0;");

                }
                if (verticalPosition > 20.0)
                {
                    verticalChange += 1.0;
                    Debug.WriteLine(" verticalPosition += 1.0;");

                }
                if (verticalPosition < -20.0)
                {
                    verticalChange -= 1.0;
                    Debug.WriteLine(" verticalPosition -= 1.0;");
                }

            }
            if (accelStarted)
            {
                horizontalChange = accelSpeedX;
                verticalChange = 0.0;
                if (((uint)num3 & 4u) != 0)
                {
                    verticalChange = 1.0;
                }
            }
            decor.SetSpeedX(horizontalChange);
            decor.SetSpeedY(verticalChange);
            decor.KeyChange(num3);
        }

        private Def.ButtonGlyph ButtonDetect(TinyPoint pos)
        {
            foreach (Def.ButtonGlyph item in ButtonGlyphs.Reverse())
            {
                int value = 0;
                if (item == Def.ButtonGlyph.PlayJump || item == Def.ButtonGlyph.PlayAction || item == Def.ButtonGlyph.PlayDown || item == Def.ButtonGlyph.PlayPause)
                {
                    value = 20;
                }
                TinyRect rect = Misc.Inflate(GetButtonRect(item), value);
                if (Misc.IsInside(rect, pos))
                {
                    return item;
                }
            }
            return Def.ButtonGlyph.None;
        }

        public void Draw()
        {
            if (!accelStarted && Phase == Def.Phase.Play)
            {
                pixmap.DrawIcon(14, 0, GetPadBounds(PadCenter, padSize / 2), 1.0, false);
                TinyPoint center = (padPressed ? padTouchPos : PadCenter);
                pixmap.DrawIcon(14, 1, GetPadBounds(center, padSize / 2), 1.0, false);
            }
            foreach (Def.ButtonGlyph buttonGlyph in ButtonGlyphs)
            {
                bool pressed = pressedGlyphs.Contains(buttonGlyph);
                bool selected = false;
                if (buttonGlyph >= Def.ButtonGlyph.InitGamerA && buttonGlyph <= Def.ButtonGlyph.InitGamerC)
                {
                    int num = (int)(buttonGlyph - 1);
                    selected = num == gameData.SelectedGamer;
                }
                if (buttonGlyph == Def.ButtonGlyph.SetupSounds)
                {
                    selected = gameData.Sounds;
                }
                if (buttonGlyph == Def.ButtonGlyph.SetupJump)
                {
                    selected = gameData.JumpRight;
                }
                if (buttonGlyph == Def.ButtonGlyph.SetupZoom)
                {
                    selected = gameData.AutoZoom;
                }
                if (buttonGlyph == Def.ButtonGlyph.SetupAccel)
                {
                    selected = gameData.AccelActive;
                }
                pixmap.DrawInputButton(GetButtonRect(buttonGlyph), buttonGlyph, pressed, selected);
            }
            if ((Phase == Def.Phase.MainSetup || Phase == Def.Phase.PlaySetup) && gameData.AccelActive)
            {
                accelSlider.Draw(pixmap);
            }
        }

        private TinyRect GetPadBounds(TinyPoint center, int radius)
        {
            TinyRect result = default(TinyRect);
            result.Left = center.X - radius;
            result.Right = center.X + radius;
            result.Top = center.Y - radius;
            result.Bottom = center.Y + radius;
            return result;
        }

        public TinyRect GetButtonRect(Def.ButtonGlyph glyph)
        {
            TinyRect drawBounds = pixmap.DrawBounds;
            double num = drawBounds.Width;
            double num2 = drawBounds.Height;
            double num3 = num2 / 5.0;
            double num4 = num2 * 140.0 / 480.0;
            double num5 = num2 / 3.5;
            if (glyph >= Def.ButtonGlyph.Cheat1 && glyph <= Def.ButtonGlyph.Cheat9)
            {
                int num6 = (int)(glyph - 35);
                TinyRect result = default(TinyRect);
                result.Left = 80 * num6;
                result.Right = 80 * (num6 + 1);
                result.Top = 0;
                result.Bottom = 80;
                return result;
            }
            switch (glyph)
            {
                case Def.ButtonGlyph.InitGamerA:
                    {
                        TinyRect result19 = default(TinyRect);
                        result19.Left = (int)(20.0 + num4 * 0.0);
                        result19.Right = (int)(20.0 + num4 * 0.5);
                        result19.Top = (int)(num2 - 20.0 - num4 * 2.1);
                        result19.Bottom = (int)(num2 - 20.0 - num4 * 1.6);
                        return result19;
                    }
                case Def.ButtonGlyph.InitGamerB:
                    {
                        TinyRect result18 = default(TinyRect);
                        result18.Left = (int)(20.0 + num4 * 0.0);
                        result18.Right = (int)(20.0 + num4 * 0.5);
                        result18.Top = (int)(num2 - 20.0 - num4 * 1.6);
                        result18.Bottom = (int)(num2 - 20.0 - num4 * 1.1);
                        return result18;
                    }
                case Def.ButtonGlyph.InitGamerC:
                    {
                        TinyRect result15 = default(TinyRect);
                        result15.Left = (int)(20.0 + num4 * 0.0);
                        result15.Right = (int)(20.0 + num4 * 0.5);
                        result15.Top = (int)(num2 - 20.0 - num4 * 1.1);
                        result15.Bottom = (int)(num2 - 20.0 - num4 * 0.6);
                        return result15;
                    }
                case Def.ButtonGlyph.InitSetup:
                    {
                        TinyRect result14 = default(TinyRect);
                        result14.Left = (int)(20.0 + num4 * 0.0);
                        result14.Right = (int)(20.0 + num4 * 0.5);
                        result14.Top = (int)(num2 - 20.0 - num4 * 0.5);
                        result14.Bottom = (int)(num2 - 20.0 - num4 * 0.0);
                        return result14;
                    }
                case Def.ButtonGlyph.InitPlay:
                    {
                        TinyRect result11 = default(TinyRect);
                        result11.Left = (int)(num - 20.0 - num4 * 1.0);
                        result11.Right = (int)(num - 20.0 - num4 * 0.0);
                        result11.Top = (int)(num2 - 40.0 - num4 * 1.0);
                        result11.Bottom = (int)(num2 - 40.0 - num4 * 0.0);
                        return result11;
                    }
                case Def.ButtonGlyph.InitBuy:
                case Def.ButtonGlyph.InitRanking:
                    {
                        TinyRect result10 = default(TinyRect);
                        result10.Left = (int)(num - 20.0 - num4 * 0.75);
                        result10.Right = (int)(num - 20.0 - num4 * 0.25);
                        result10.Top = (int)(num2 - 20.0 - num4 * 2.1);
                        result10.Bottom = (int)(num2 - 20.0 - num4 * 1.6);
                        return result10;
                    }
                case Def.ButtonGlyph.PauseMenu:
                    {
                        TinyRect result37 = default(TinyRect);
                        result37.Left = (int)((double)PixmapOrigin.X + num4 * -0.21);
                        result37.Right = (int)((double)PixmapOrigin.X + num4 * 0.79);
                        result37.Top = (int)((double)PixmapOrigin.Y + num4 * 2.2);
                        result37.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.2);
                        return result37;
                    }
                case Def.ButtonGlyph.PauseBack:
                    {
                        TinyRect result36 = default(TinyRect);
                        result36.Left = (int)((double)PixmapOrigin.X + num4 * 0.79);
                        result36.Right = (int)((double)PixmapOrigin.X + num4 * 1.79);
                        result36.Top = (int)((double)PixmapOrigin.Y + num4 * 2.2);
                        result36.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.2);
                        return result36;
                    }
                case Def.ButtonGlyph.PauseSetup:
                    {
                        TinyRect result35 = default(TinyRect);
                        result35.Left = (int)((double)PixmapOrigin.X + num4 * 1.79);
                        result35.Right = (int)((double)PixmapOrigin.X + num4 * 2.79);
                        result35.Top = (int)((double)PixmapOrigin.Y + num4 * 2.2);
                        result35.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.2);
                        return result35;
                    }
                case Def.ButtonGlyph.PauseRestart:
                    {
                        TinyRect result34 = default(TinyRect);
                        result34.Left = (int)((double)PixmapOrigin.X + num4 * 2.79);
                        result34.Right = (int)((double)PixmapOrigin.X + num4 * 3.79);
                        result34.Top = (int)((double)PixmapOrigin.Y + num4 * 2.2);
                        result34.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.2);
                        return result34;
                    }
                case Def.ButtonGlyph.PauseContinue:
                    {
                        TinyRect result33 = default(TinyRect);
                        result33.Left = (int)((double)PixmapOrigin.X + num4 * 3.79);
                        result33.Right = (int)((double)PixmapOrigin.X + num4 * 4.79);
                        result33.Top = (int)((double)PixmapOrigin.Y + num4 * 2.2);
                        result33.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.2);
                        return result33;
                    }
                case Def.ButtonGlyph.ResumeMenu:
                    {
                        TinyRect result32 = default(TinyRect);
                        result32.Left = (int)((double)PixmapOrigin.X + num4 * 1.29);
                        result32.Right = (int)((double)PixmapOrigin.X + num4 * 2.29);
                        result32.Top = (int)((double)PixmapOrigin.Y + num4 * 2.2);
                        result32.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.2);
                        return result32;
                    }
                case Def.ButtonGlyph.ResumeContinue:
                    {
                        TinyRect result31 = default(TinyRect);
                        result31.Left = (int)((double)PixmapOrigin.X + num4 * 2.29);
                        result31.Right = (int)((double)PixmapOrigin.X + num4 * 3.29);
                        result31.Top = (int)((double)PixmapOrigin.Y + num4 * 2.2);
                        result31.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.2);
                        return result31;
                    }
                case Def.ButtonGlyph.WinLostReturn:
                    {
                        TinyRect result30 = default(TinyRect);
                        result30.Left = (int)((double)PixmapOrigin.X + num - num3 * 2.2);
                        result30.Right = (int)((double)PixmapOrigin.X + num - num3 * 1.2);
                        result30.Top = (int)((double)PixmapOrigin.Y + num3 * 0.2);
                        result30.Bottom = (int)((double)PixmapOrigin.Y + num3 * 1.2);
                        return result30;
                    }
                case Def.ButtonGlyph.TrialBuy:
                    {
                        TinyRect result29 = default(TinyRect);
                        result29.Left = (int)((double)PixmapOrigin.X + num4 * 2.5);
                        result29.Right = (int)((double)PixmapOrigin.X + num4 * 3.5);
                        result29.Top = (int)((double)PixmapOrigin.Y + num4 * 2.1);
                        result29.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.1);
                        return result29;
                    }
                case Def.ButtonGlyph.TrialCancel:
                    {
                        TinyRect result28 = default(TinyRect);
                        result28.Left = (int)((double)PixmapOrigin.X + num4 * 3.5);
                        result28.Right = (int)((double)PixmapOrigin.X + num4 * 4.5);
                        result28.Top = (int)((double)PixmapOrigin.Y + num4 * 2.1);
                        result28.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.1);
                        return result28;
                    }
                case Def.ButtonGlyph.RankingContinue:
                    {
                        TinyRect result27 = default(TinyRect);
                        result27.Left = (int)((double)PixmapOrigin.X + num4 * 3.5);
                        result27.Right = (int)((double)PixmapOrigin.X + num4 * 4.5);
                        result27.Top = (int)((double)PixmapOrigin.Y + num4 * 2.1);
                        result27.Bottom = (int)((double)PixmapOrigin.Y + num4 * 3.1);
                        return result27;
                    }
                case Def.ButtonGlyph.SetupSounds:
                    {
                        TinyRect result26 = default(TinyRect);
                        result26.Left = (int)(20.0 + num4 * 0.0);
                        result26.Right = (int)(20.0 + num4 * 0.5);
                        result26.Top = (int)(num2 - 20.0 - num4 * 2.0);
                        result26.Bottom = (int)(num2 - 20.0 - num4 * 1.5);
                        return result26;
                    }
                case Def.ButtonGlyph.SetupJump:
                    {
                        TinyRect result25 = default(TinyRect);
                        result25.Left = (int)(20.0 + num4 * 0.0);
                        result25.Right = (int)(20.0 + num4 * 0.5);
                        result25.Top = (int)(num2 - 20.0 - num4 * 1.5);
                        result25.Bottom = (int)(num2 - 20.0 - num4 * 1.0);
                        return result25;
                    }
                case Def.ButtonGlyph.SetupZoom:
                    {
                        TinyRect result24 = default(TinyRect);
                        result24.Left = (int)(20.0 + num4 * 0.0);
                        result24.Right = (int)(20.0 + num4 * 0.5);
                        result24.Top = (int)(num2 - 20.0 - num4 * 1.0);
                        result24.Bottom = (int)(num2 - 20.0 - num4 * 0.5);
                        return result24;
                    }
                case Def.ButtonGlyph.SetupAccel:
                    {
                        TinyRect result23 = default(TinyRect);
                        result23.Left = (int)(20.0 + num4 * 0.0);
                        result23.Right = (int)(20.0 + num4 * 0.5);
                        result23.Top = (int)(num2 - 20.0 - num4 * 0.5);
                        result23.Bottom = (int)(num2 - 20.0 - num4 * 0.0);
                        return result23;
                    }
                case Def.ButtonGlyph.SetupReset:
                    {
                        TinyRect result22 = default(TinyRect);
                        result22.Left = (int)(450.0 + num4 * 0.0);
                        result22.Right = (int)(450.0 + num4 * 0.5);
                        result22.Top = (int)(num2 - 20.0 - num4 * 2.0);
                        result22.Bottom = (int)(num2 - 20.0 - num4 * 1.5);
                        return result22;
                    }
                case Def.ButtonGlyph.SetupReturn:
                    {
                        TinyRect result21 = default(TinyRect);
                        result21.Left = (int)(num - 20.0 - num4 * 0.8);
                        result21.Right = (int)(num - 20.0 - num4 * 0.0);
                        result21.Top = (int)(num2 - 20.0 - num4 * 0.8);
                        result21.Bottom = (int)(num2 - 20.0 - num4 * 0.0);
                        return result21;
                    }
                case Def.ButtonGlyph.PlayPause:
                    {
                        TinyRect result20 = default(TinyRect);
                        result20.Left = (int)(num - num3 * 0.7);
                        result20.Right = (int)(num - num3 * 0.2);
                        result20.Top = (int)(num3 * 0.2);
                        result20.Bottom = (int)(num3 * 0.7);
                        return result20;
                    }
                case Def.ButtonGlyph.PlayAction:
                    {
                        if (gameData.JumpRight)
                        {
                            TinyRect result16 = default(TinyRect);
                            result16.Left = (int)((double)drawBounds.Width - num3 * 1.2);
                            result16.Right = (int)((double)drawBounds.Width - num3 * 0.2);
                            result16.Top = (int)(num2 - num3 * 2.6);
                            result16.Bottom = (int)(num2 - num3 * 1.6);
                            return result16;
                        }
                        TinyRect result17 = default(TinyRect);
                        result17.Left = (int)(num3 * 0.2);
                        result17.Right = (int)(num3 * 1.2);
                        result17.Top = (int)(num2 - num3 * 2.6);
                        result17.Bottom = (int)(num2 - num3 * 1.6);
                        return result17;
                    }
                case Def.ButtonGlyph.PlayJump:
                    {
                        if (gameData.JumpRight)
                        {
                            TinyRect result12 = default(TinyRect);
                            result12.Left = (int)((double)drawBounds.Width - num3 * 1.2);
                            result12.Right = (int)((double)drawBounds.Width - num3 * 0.2);
                            result12.Top = (int)(num2 - num3 * 1.2);
                            result12.Bottom = (int)(num2 - num3 * 0.2);
                            return result12;
                        }
                        TinyRect result13 = default(TinyRect);
                        result13.Left = (int)(num3 * 0.2);
                        result13.Right = (int)(num3 * 1.2);
                        result13.Top = (int)(num2 - num3 * 1.2);
                        result13.Bottom = (int)(num2 - num3 * 0.2);
                        return result13;
                    }
                case Def.ButtonGlyph.PlayDown:
                    {
                        if (gameData.JumpRight)
                        {
                            TinyRect result8 = default(TinyRect);
                            result8.Left = (int)(num3 * 0.2);
                            result8.Right = (int)(num3 * 1.2);
                            result8.Top = (int)(num2 - num3 * 1.2);
                            result8.Bottom = (int)(num2 - num3 * 0.2);
                            return result8;
                        }
                        TinyRect result9 = default(TinyRect);
                        result9.Left = (int)((double)drawBounds.Width - num3 * 1.2);
                        result9.Right = (int)((double)drawBounds.Width - num3 * 0.2);
                        result9.Top = (int)(num2 - num3 * 1.2);
                        result9.Bottom = (int)(num2 - num3 * 0.2);
                        return result9;
                    }
                case Def.ButtonGlyph.Cheat11:
                    {
                        TinyRect result7 = default(TinyRect);
                        result7.Left = (int)(num5 * 0.0);
                        result7.Right = (int)(num5 * 1.0);
                        result7.Top = (int)(num5 * 0.0);
                        result7.Bottom = (int)(num5 * 1.0);
                        return result7;
                    }
                case Def.ButtonGlyph.Cheat12:
                    {
                        TinyRect result6 = default(TinyRect);
                        result6.Left = (int)(num5 * 0.0);
                        result6.Right = (int)(num5 * 1.0);
                        result6.Top = (int)(num5 * 1.0);
                        result6.Bottom = (int)(num5 * 2.0);
                        return result6;
                    }
                case Def.ButtonGlyph.Cheat21:
                    {
                        TinyRect result5 = default(TinyRect);
                        result5.Left = (int)(num5 * 1.0);
                        result5.Right = (int)(num5 * 2.0);
                        result5.Top = (int)(num5 * 0.0);
                        result5.Bottom = (int)(num5 * 1.0);
                        return result5;
                    }
                case Def.ButtonGlyph.Cheat22:
                    {
                        TinyRect result4 = default(TinyRect);
                        result4.Left = (int)(num5 * 1.0);
                        result4.Right = (int)(num5 * 2.0);
                        result4.Top = (int)(num5 * 1.0);
                        result4.Bottom = (int)(num5 * 2.0);
                        return result4;
                    }
                case Def.ButtonGlyph.Cheat31:
                    {
                        TinyRect result3 = default(TinyRect);
                        result3.Left = (int)(num5 * 2.0);
                        result3.Right = (int)(num5 * 3.0);
                        result3.Top = (int)(num5 * 0.0);
                        result3.Bottom = (int)(num5 * 1.0);
                        return result3;
                    }
                case Def.ButtonGlyph.Cheat32:
                    {
                        TinyRect result2 = default(TinyRect);
                        result2.Left = (int)(num5 * 2.0);
                        result2.Right = (int)(num5 * 3.0);
                        result2.Top = (int)(num5 * 1.0);
                        result2.Bottom = (int)(num5 * 2.0);
                        return result2;
                    }
                default:
                    return default(TinyRect);
            }
        }

        private void StartAccel()
        {
            try
            {
                accelSensor.Start();
                accelStarted = true;
            }
            catch (AccelerometerFailedException)
            {
                accelStarted = false;
            }
            catch (UnauthorizedAccessException)
            {
                accelStarted = false;
            }
        }

        private void StopAccel()
        {
            if (accelStarted)
            {
                try
                {
                    accelSensor.Stop();
                }
                catch (AccelerometerFailedException)
                {
                }
                accelStarted = false;
            }
        }


        private void HandleAccelSensorCurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)

            AccelerometerReading sensorReading = e.SensorReading;
            float y = ((AccelerometerReading)(sensorReading)).Acceleration.Y;
            float num = (1f - (float)gameData.AccelSensitivity) * 0.06f + 0.04f;
            float num2 = (accelLastState ? (num * 0.6f) : num);
            if (y > num2)
            {
                accelSpeedX = 0.0 - Math.Min((double)y * 0.25 / (double)num + 0.25, 1.0);
            }
            else if (y < 0f - num2)
            {
                accelSpeedX = Math.Min((double)(0f - y) * 0.25 / (double)num + 0.25, 1.0);
            }
            else
            {
                accelSpeedX = 0.0;
            }
            accelLastState = accelSpeedX != 0.0;
            if (accelWaitZero)
            {
                if (accelSpeedX == 0.0)
                {
                    accelWaitZero = false;
                }
                else
                {
                    accelSpeedX = 0.0;
                }
            }
        }
    }

}