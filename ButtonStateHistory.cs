﻿using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;

namespace InputVisualizer
{
    public class ButtonStateHistory
    {
        public List<ButtonStateValue> StateChangeHistory { get; private set; } = new List<ButtonStateValue>();
        public Color Color { get; set; }
        private object _modifyLock = new object();
        public ButtonType UnmappedButtonType { get; set; }

        public void AddStateChange(bool state, DateTime time)
        {
            lock (_modifyLock)
            {
                if (StateChangeHistory.Any())
                {
                    var last = StateChangeHistory.Last();
                    last.EndTime = time;
                    last.Completed = true;
                }
                StateChangeHistory.Add(new ButtonStateValue { IsPressed = state, StartTime = time });
            }
        }

        public void RemoveOldStateChanges(double ms)
        {
            lock (_modifyLock)
            {
                var removeItems = new List<ButtonStateValue>();
                foreach (var change in StateChangeHistory)
                {
                    if (change.Completed && change.EndTime < DateTime.Now.AddMilliseconds(-ms))
                    {
                        removeItems.Add(change);
                    }
                    if (!change.IsPressed && change.StartTime < DateTime.Now.AddMilliseconds(-ms))
                    {
                        removeItems.Add(change);
                    }
                }
                foreach (var item in removeItems)
                {
                    StateChangeHistory.Remove(item);
                }
            }
        }

        public bool IsPressed()
        {
            lock (_modifyLock)
            {
                if (!StateChangeHistory.Any())
                {
                    return false;
                }
                return StateChangeHistory.Last().IsPressed;
            }
        }

        public TimeSpan PressedElapsed()
        {
            lock (_modifyLock)
            {
                if (!StateChangeHistory.Any())
                {
                    return TimeSpan.Zero;
                }
                var sc = StateChangeHistory.Last();
                if (!sc.IsPressed)
                {
                    return TimeSpan.Zero;
                }
                return DateTime.Now - sc.StartTime;
            }
        }

        public int GetPressedLastSecond()
        {
            lock (_modifyLock)
            {
                var frequency = 0;
                var oneSecondAgo = DateTime.Now.AddSeconds(-1);

                var numChanges = StateChangeHistory.Count;
                for (var i = numChanges - 1; i >= 0; i--)
                {
                    var sc = StateChangeHistory[i];
                    if (sc.StartTime < oneSecondAgo)
                    {
                        break;
                    }
                    if (sc.IsPressed)
                    {
                        frequency++;
                    }
                }
                return frequency;
            }
        }
    }
}
