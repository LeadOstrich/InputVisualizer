﻿using Myra.Graphics2D.UI;
using Myra;
using System;
using System.Collections.Generic;
using System.Linq;
using Myra.Graphics2D;
using Microsoft.Xna.Framework;
using InputVisualizer.Config;
using Myra.Graphics2D.UI.ColorPicker;
using System.IO.Ports;
using Microsoft.Xna.Framework.Input;

namespace InputVisualizer.UI
{
    public class GameUI
    {
        private Desktop _desktop;
        private ViewerConfig _config;
        private GameState _gameState;

        private bool _listeningForInput = false;
        private bool _listeningCancelPressed = false;
        private GamepadButtonMapping _listeningMapping;
        private TextButton _listeningButton;
        private Grid _listeningGrid;

        public bool ListeningForInput => _listeningForInput;

        public event EventHandler<InputSourceChangedEventArgs> InputSourceChanged;
        public event EventHandler GamepadSettingsUpdated;
        public event EventHandler RetroSpySettingsUpdated;
        public event EventHandler DisplaySettingsUpdated;

        public GameUI(Game game, ViewerConfig config, GameState gameState)
        {
            MyraEnvironment.Game = game;
            _config = config;
            _gameState = gameState;
        }

        public void Init(Dictionary<string, GamePadInfo> systemGamepads)
        {

            var grid = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
                Padding = new Thickness(3),
                Margin = new Thickness(3),
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            var inputSourceCombo = new ComboBox
            {
                GridColumn = 0,
                GridRow = 0,
                Padding = new Thickness(2),
            };

            foreach (var kvp in systemGamepads)
            {
                var name = kvp.Value.Name.Length > 32 ? kvp.Value.Name.Substring(0, 32) : kvp.Value.Name;
                inputSourceCombo.Items.Add(new ListItem(name, Color.White, kvp.Key));
            }
            inputSourceCombo.Items.Add(new ListItem("RetroSpy", Color.White, "spy"));

            foreach (var item in inputSourceCombo.Items)
            {
                if (_config.CurrentInputSource == (string)item.Tag)
                {
                    inputSourceCombo.SelectedItem = item;
                }
            }

            inputSourceCombo.SelectedIndexChanged += (s, a) =>
            {
                if (InputSourceChanged != null)
                {
                    var args = new InputSourceChangedEventArgs() { InputSourceId = (string)inputSourceCombo.SelectedItem.Tag };
                    InputSourceChanged(this, args);
                }
            };

            grid.Widgets.Add(inputSourceCombo);

            var configureInputButton = new TextButton
            {
                GridColumn = 1,
                GridRow = 0,
                Text = "Input",
                Padding = new Thickness(2)
            };

            configureInputButton.Click += (s, a) =>
            {
                if (_gameState.CurrentInputMode == InputMode.RetroSpy)
                {
                    ShowConfigureRetroSpyDialog();
                }
                else
                {
                    ShowConfigureGamePadDialog(systemGamepads);
                }
            };

            grid.Widgets.Add(configureInputButton);

            var configureDisplayButton = new TextButton
            {
                GridColumn = 2,
                GridRow = 0,
                Text = "Display",
                Padding = new Thickness(2)
            };
            configureDisplayButton.Click += (s, a) =>
            {
                ShowConfigureDisplayDialog();
            };
            grid.Widgets.Add(configureDisplayButton);

            var aboutButton = new TextButton
            {
                GridColumn = 3,
                GridRow = 0,
                Text = "?",
                Padding = new Thickness(2),
                Width = 30
            };
            aboutButton.Click += (s, a) =>
            {
                ShowAboutDialog();
            };
            grid.Widgets.Add(aboutButton);

            var container = new HorizontalStackPanel();

            var menuBar = new HorizontalMenu()
            {
                Padding = new Thickness(1),
            };

            var menuItemInputs = new MenuItem();
            menuItemInputs.Text = "Input";
            menuItemInputs.Id = "menuItemInputs";
            menuItemInputs.Selected += (s, a) =>
            {
                if (_gameState.CurrentInputMode == InputMode.RetroSpy)
                {
                    ShowConfigureRetroSpyDialog();
                }
                else
                {
                    ShowConfigureGamePadDialog(systemGamepads);
                }
            };
            container.Widgets.Add(inputSourceCombo);
            container.Widgets.Add(menuBar);

            var menuItemDisplay = new MenuItem();
            menuItemDisplay.Text = "Display";
            menuItemDisplay.Id = "menuItemDisplay";

            menuItemDisplay.Selected += (s, a) =>
            {
                ShowConfigureDisplayDialog();
            };

            var menuItemSettings = new MenuItem();
            menuItemSettings.Text = "Configure";
            menuItemSettings.Id = "menuItemSettings";
            menuItemSettings.Items.Add(menuItemInputs);
            menuItemSettings.Items.Add(menuItemDisplay);

            var menuItemAbout = new MenuItem();
            menuItemAbout.Text = "About";
            menuItemAbout.Id = "menuItemAbout";
            menuItemAbout.Selected += (s, a) =>
            {
                ShowAboutDialog();
            };

            menuBar.Items.Add(menuItemSettings);
            menuBar.Items.Add(menuItemAbout);

            _desktop = new Desktop();
            _desktop.Root = container;
            _desktop.Root.VerticalAlignment = VerticalAlignment.Top;
            _desktop.Root.HorizontalAlignment = HorizontalAlignment.Left;
        }

        public void Render()
        {
            try
            {
                _desktop.Render();
            }
            catch (Exception)
            {

            }
        }

        public void CheckForListeningInput()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                _listeningForInput = false;
                _listeningCancelPressed = true;
                _listeningButton.Text = _listeningMapping.ButtonType.ToString();
                return;
            }

            var buttonDetected = ButtonType.NONE;
            if (GamePad.GetState(_gameState.CurrentPlayerIndex).DPad.Up == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.UP;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).DPad.Down == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.DOWN;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).DPad.Left == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.LEFT;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).DPad.Right == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.RIGHT;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).Buttons.A == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.A;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).Buttons.B == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.B;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).Buttons.X == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.X;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).Buttons.Y == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.Y;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).Buttons.LeftShoulder == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.L;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).Buttons.RightShoulder == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.R;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).Buttons.Back == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.SELECT;
            }
            else if (GamePad.GetState(_gameState.CurrentPlayerIndex).Buttons.Start == ButtonState.Pressed)
            {
                buttonDetected = ButtonType.START;
            }

            if (buttonDetected != ButtonType.NONE)
            {
                _listeningMapping.MappedButtonType = buttonDetected;
                _listeningButton.Text = buttonDetected.ToString();

                foreach (var mapping in _gameState.ActiveGamepadConfig.ButtonMappings)
                {
                    if (mapping == _listeningMapping)
                    {
                        continue;
                    }
                    if (mapping.MappedButtonType == buttonDetected)
                    {
                        mapping.MappedButtonType = ButtonType.NONE;
                        var textBox = _listeningGrid.Widgets.OfType<TextButton>().FirstOrDefault(b => b.Tag == mapping);
                        if (textBox != null)
                        {
                            textBox.Text = mapping.MappedButtonType.ToString();
                        }
                    }
                }
                _listeningForInput = false;
            }
        }

        private void ShowConfigureGamePadDialog(Dictionary<string, GamePadInfo> systemGamepads)
        {
            var buttonMapWidgets = new List<Widget>();

            var gamePadName = systemGamepads[_gameState.ActiveGamepadConfig.Id].Name;
            var name = gamePadName.Length > 32 ? gamePadName.Substring(0, 32) : gamePadName;
            var dialog = new Dialog
            {
                Title = $"{name} Config",
                TitleTextColor = Color.DarkSeaGreen
            };

            var grid = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
                Padding = new Thickness(3),
                Margin = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            var label2 = new Label
            {
                Text = "Style:",
                GridRow = 0,
                GridColumn = 0,
                GridColumnSpan = 2,
            };
            grid.Widgets.Add(label2);
            var styleComboBox = new ComboBox()
            {
                GridRow = 0,
                GridColumn = 2,
                GridColumnSpan = 3,
            };

            foreach (GamepadStyle value in Enum.GetValues(typeof(GamepadStyle)))
            {
                var item = new ListItem(value.ToString(), Color.White, value);
                styleComboBox.Items.Add(item);
                if (_gameState.ActiveGamepadConfig.Style == value)
                {
                    styleComboBox.SelectedItem = item;
                }
            }
            styleComboBox.SelectedIndexChanged += (o, e) =>
            {
                _gameState.ActiveGamepadConfig.Style = (GamepadStyle)styleComboBox.SelectedItem.Tag;
                _gameState.ActiveGamepadConfig.GenerateButtonMappings();
                DrawButtonMappings(_gameState.ActiveGamepadConfig.ButtonMappings, grid, buttonMapWidgets, 2, showMapButton: true);
            };
            grid.Widgets.Add(styleComboBox);

            var mapLabelVisible = new Label
            {
                Text = "Visible",
                GridRow = 1,
                GridColumn = 0
            };
            var mapLabelButton = new Label
            {
                Text = "Button",
                GridRow = 1,
                GridColumn = 1
            };
            var mapLabelButtonMap = new Label
            {
                Text = "Mapped To",
                GridRow = 1,
                GridColumn = 2
            };
            var mapLabelColor = new Label
            {
                Text = "Color",
                GridRow = 1,
                GridColumn = 3
            };
            var mapLabelOrder = new Label
            {
                Text = "Order",
                GridRow = 1,
                GridColumn = 4,
                GridColumnSpan = 2,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            grid.Widgets.Add(mapLabelVisible);
            grid.Widgets.Add(mapLabelButton);
            grid.Widgets.Add(mapLabelButtonMap);
            grid.Widgets.Add(mapLabelColor);
            grid.Widgets.Add(mapLabelOrder);

            DrawButtonMappings(_gameState.ActiveGamepadConfig.ButtonMappings, grid, buttonMapWidgets, 2, showMapButton: true);

            dialog.Content = grid;
            dialog.Closing += (s, a) =>
            {
                if (_listeningForInput)
                {
                    var messageBox = Dialog.CreateMessageBox("Hey you!", "Finish mapping button or hit DEL to cancel");
                    messageBox.ShowModal(_desktop);
                    a.Cancel = true;
                }
                else if (_listeningCancelPressed)
                {
                    a.Cancel = true;
                    _listeningCancelPressed = false;
                }
            };
            dialog.Closed += (s, a) =>
            {
                _listeningForInput = false;
                if (!dialog.Result)
                {
                    return;
                }

                if( GamepadSettingsUpdated != null )
                {
                    GamepadSettingsUpdated(this, EventArgs.Empty);
                }
            };
            dialog.ShowModal(_desktop);
        }

        private void ShowConfigureRetroSpyDialog()
        {
            var buttonMapWidgets = new List<Widget>();

            var dialog = new Dialog
            {
                Title = "RetroSpy Config",
                TitleTextColor = Color.DarkSeaGreen
            };

            var grid = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
                Padding = new Thickness(3),
                Margin = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            var label1 = new Label
            {
                Text = "COM Port:",
                GridColumnSpan = 2
            };
            grid.Widgets.Add(label1);

            var comPortComboBox = new ComboBox()
            {
                GridRow = 0,
                GridColumn = 2,
                GridColumnSpan = 2,
            };

            foreach (var name in SerialPort.GetPortNames())
            {
                var item = new ListItem(name, Color.White, name);
                comPortComboBox.Items.Add(item);
                if (string.Equals(_config.RetroSpyConfig.ComPortName, name, StringComparison.OrdinalIgnoreCase))
                {
                    comPortComboBox.SelectedItem = item;
                }
            }
            grid.Widgets.Add(comPortComboBox);

            var label2 = new Label
            {
                Text = "Style:",
                GridRow = 1,
                GridColumn = 0,
                GridColumnSpan = 2,
            };
            grid.Widgets.Add(label2);
            var styleComboBox = new ComboBox()
            {
                GridRow = 1,
                GridColumn = 2,
                GridColumnSpan = 2,
            };

            foreach (RetroSpyControllerType value in Enum.GetValues(typeof(RetroSpyControllerType)))
            {
                var item = new ListItem(value.ToString(), Color.White, value);
                styleComboBox.Items.Add(item);
                if (_config.RetroSpyConfig.ControllerType == value)
                {
                    styleComboBox.SelectedItem = item;
                }
            }
            styleComboBox.SelectedIndexChanged += (o, e) =>
            {
                _config.RetroSpyConfig.ControllerType = (RetroSpyControllerType)styleComboBox.SelectedItem.Tag;
                DrawButtonMappings(_config.RetroSpyConfig.GetMappingSet(_config.RetroSpyConfig.ControllerType).ButtonMappings, grid, buttonMapWidgets, 3);
            };
            grid.Widgets.Add(styleComboBox);

            var mapLabelVisible = new Label
            {
                Text = "Visible",
                GridRow = 2,
                GridColumn = 0
            };
            var mapLabelButton = new Label
            {
                Text = "Button",
                GridRow = 2,
                GridColumn = 1
            };
            var mapLabelColor = new Label
            {
                Text = "Color",
                GridRow = 2,
                GridColumn = 2
            };
            var mapLabelOrder = new Label
            {
                Text = "Order",
                GridRow = 2,
                GridColumn = 3,
                GridColumnSpan = 2,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            grid.Widgets.Add(mapLabelVisible);
            grid.Widgets.Add(mapLabelButton);
            grid.Widgets.Add(mapLabelColor);
            grid.Widgets.Add(mapLabelOrder);

            DrawButtonMappings(_config.RetroSpyConfig.GetMappingSet(_config.RetroSpyConfig.ControllerType).ButtonMappings, grid, buttonMapWidgets, 3);

            dialog.Content = grid;
            dialog.Closed += (s, a) =>
            {
                if (!dialog.Result)
                {
                    return;
                }
                if (comPortComboBox.SelectedItem != null)
                {
                    _config.RetroSpyConfig.ComPortName = (string)comPortComboBox.SelectedItem.Tag;
                }
                _config.RetroSpyConfig.ControllerType = (RetroSpyControllerType)styleComboBox.SelectedItem.Tag;

                if (RetroSpySettingsUpdated != null)
                {
                    RetroSpySettingsUpdated(this, EventArgs.Empty);
                }
            };
            dialog.ShowModal(_desktop);
        }

        private void DrawButtonMappings(List<GamepadButtonMapping> mappings, Grid grid, List<Widget> currentWidgets, int gridStartRow, bool showMapButton = false)
        {
            var currGridRow = gridStartRow;
            var lastGridRow = gridStartRow + mappings.Count - 1;

            foreach (var widget in currentWidgets)
            {
                grid.Widgets.Remove(widget);
            }
            currentWidgets.Clear();

            foreach (var mapping in mappings.OrderBy(m => m.Order))
            {
                var currColumn = 0;

                var visibleCheck = new CheckBox
                {
                    IsChecked = mapping.IsVisible,
                    GridRow = currGridRow,
                    GridColumn = currColumn
                };
                visibleCheck.Click += (s, e) =>
                {
                    mapping.IsVisible = visibleCheck.IsChecked;
                };
                currentWidgets.Add(visibleCheck);
                currColumn++;
                var buttonLabel = new Label
                {
                    Text = mapping.Label,
                    GridRow = currGridRow,
                    GridColumn = currColumn
                };
                currentWidgets.Add(buttonLabel);
                currColumn++;

                if (showMapButton)
                {
                    var mapButton = new TextButton
                    {
                        GridRow = currGridRow,
                        GridColumn = currColumn,
                        Text = mapping.MappedButtonType.ToString(),
                        Padding = new Thickness(2),
                        Tag = mapping
                    };
                    mapButton.Click += (s, e) =>
                    {
                        if (_listeningForInput)
                        {
                            var messageBox = Dialog.CreateMessageBox("Hey you!", "Finish mapping button or hit ESC to cancel");
                            messageBox.ShowModal(_desktop);
                            return;
                        }
                        _listeningForInput = true;
                        _listeningButton = mapButton;
                        _listeningButton.Text = "...";
                        _listeningMapping = mapping;
                        _listeningGrid = grid;
                    };

                    currentWidgets.Add(mapButton);
                    currColumn++;
                }

                var colorButton = new TextButton
                {
                    GridRow = currGridRow,
                    GridColumn = currColumn,
                    Text = "Color",
                    Padding = new Thickness(2),
                    TextColor = mapping.Color,
                };
                colorButton.Click += (s, e) =>
                {
                    ChooseColor(mapping, colorButton);
                };
                currentWidgets.Add(colorButton);
                currColumn++;

                if (currGridRow > gridStartRow)
                {
                    var upButton = new TextButton
                    {
                        GridColumn = currColumn,
                        GridRow = currGridRow,
                        Width = 30,
                        Text = "↑",
                        HorizontalAlignment = HorizontalAlignment.Right
                    };

                    upButton.Click += (s, e) =>
                    {
                        mappings = UpdateOrder(mappings, mapping, goUp: true);
                        DrawButtonMappings(mappings, grid, currentWidgets, gridStartRow, showMapButton);
                    };
                    currentWidgets.Add(upButton);
                }
                currColumn++;

                if (currGridRow < lastGridRow)
                {
                    var downButton = new TextButton
                    {
                        GridColumn = currColumn,
                        GridRow = currGridRow,
                        Width = 30,
                        Text = "↓",
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    downButton.Click += (s, e) =>
                    {
                        mappings = UpdateOrder(mappings, mapping, goUp: false);
                        DrawButtonMappings(mappings, grid, currentWidgets, gridStartRow, showMapButton);
                    };
                    currentWidgets.Add(downButton);
                }
                currGridRow++;
            }

            foreach (var widget in currentWidgets)
            {
                grid.AddChild(widget);
            }
        }

        private static List<GamepadButtonMapping> UpdateOrder(List<GamepadButtonMapping> mappings, GamepadButtonMapping targetMapping, bool goUp)
        {
            var inOrder = mappings.OrderBy(m => m.Order).ToList();
            var currIndex = inOrder.IndexOf(targetMapping);
            var targetIndex = 0;
            if (goUp)
            {
                targetIndex = currIndex - 1;
                if (targetIndex < 0)
                {
                    targetIndex = 0;
                }
            }
            else
            {
                targetIndex = currIndex + 1;
                if (targetIndex > mappings.Count - 1)
                {
                    targetIndex = mappings.Count - 1;
                }
            }

            inOrder.Remove(targetMapping);
            inOrder.Insert(targetIndex, targetMapping);

            for (var i = 0; i < inOrder.Count; i++)
            {
                inOrder[i].Order = i;
            }
            return inOrder;
        }

        private void ShowAboutDialog()
        {
            var dialog = new Dialog
            {
                Title = "About",
                TitleTextColor = Color.DarkSeaGreen
            };

            var grid = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
                Padding = new Thickness(3),
                Margin = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            var infoLabel = new Label()
            {
                GridRow = 0,
                GridColumn = 0,
                Text = "Author:",
                HorizontalAlignment = HorizontalAlignment.Left,

            };
            var infoLabel2 = new Label()
            {
                GridRow = 0,
                GridColumn = 1,
                Text = "KungFusedMike",
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            var infoLabel3 = new Label()
            {
                GridRow = 1,
                GridColumn = 0,
                Text = "Email:",
                HorizontalAlignment = HorizontalAlignment.Left,

            };
            var infoLabel4 = new Label()
            {
                GridRow = 1,
                GridColumn = 1,
                Text = "kungfusedmike@gmail.com",
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            grid.Widgets.Add(infoLabel);
            grid.Widgets.Add(infoLabel2);
            grid.Widgets.Add(infoLabel3);
            grid.Widgets.Add(infoLabel4);

            dialog.Content = grid;
            dialog.ShowModal(_desktop);
        }

        private void ShowConfigureDisplayDialog()
        {
            var dialog = new Dialog
            {
                Title = "Display Config",
                TitleTextColor = Color.DarkSeaGreen
            };

            var grid = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
                Padding = new Thickness(3),
                Margin = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            var label1 = new Label
            {
                Text = "Line Length:",
            };
            grid.Widgets.Add(label1);

            var displayWidthText = new TextBox()
            {
                GridRow = 0,
                GridColumn = 1,
                Text = _config.DisplayConfig.LineLength.ToString(),
                Width = 50
            };
            grid.Widgets.Add(displayWidthText);

            var labelSpeed = new Label
            {
                Text = "Speed:",
                GridRow = 1
            };
            grid.Widgets.Add(labelSpeed);

            var displaySpeedSpin = new HorizontalSlider()
            {
                GridRow = 1,
                GridColumn = 1,
                Value = _config.DisplayConfig.Speed,
                Minimum = 1,
                Maximum = 11,
                Width = 150
            };
            grid.Widgets.Add(displaySpeedSpin);

            var labelTurnOffLineSpeed = new Label
            {
                Text = "Dim Line Delay:",
                GridRow = 2
            };
            grid.Widgets.Add(labelTurnOffLineSpeed);

            var turnOffLineSpeedSpin = new HorizontalSlider()
            {
                GridRow = 2,
                GridColumn = 1,
                Value = _config.DisplayConfig.TurnOffLineSpeed / 50.0f,
                Minimum = 0,
                Maximum = 100,
                Width = 150
            };
            grid.Widgets.Add(turnOffLineSpeedSpin);

            var label2 = new Label
            {
                Text = "Show Duration Min Seconds:",
                GridRow = 3
            };
            grid.Widgets.Add(label2);
            var pressThresholdText = new TextBox()
            {
                GridRow = 3,
                GridColumn = 1,
                Text = _config.DisplayConfig.MinDisplayDuration.ToString(),
                Width = 50
            };
            grid.Widgets.Add(pressThresholdText);

            var label3 = new Label
            {
                Text = "Show Frequency Min Value:",
                GridRow = 4
            };
            grid.Widgets.Add(label3);
            var frequencyThresholdText = new TextBox()
            {
                GridRow = 4,
                GridColumn = 1,
                Text = _config.DisplayConfig.MinDisplayFrequency.ToString(),
                Width = 50
            };
            grid.Widgets.Add(frequencyThresholdText);

            var label4 = new Label
            {
                Text = "Display Durations:",
                GridRow = 5
            };
            grid.Widgets.Add(label4);
            var displayDurationCheck = new CheckBox()
            {
                GridRow = 5,
                GridColumn = 1,
                IsChecked = _config.DisplayConfig.DisplayDuration,
            };
            grid.Widgets.Add(displayDurationCheck);

            var label5 = new Label
            {
                Text = "Display Frequency Last Second:",
                GridRow = 6
            };
            grid.Widgets.Add(label5);
            var displayFrequencyCheck = new CheckBox()
            {
                GridRow = 6,
                GridColumn = 1,
                IsChecked = _config.DisplayConfig.DisplayFrequency,
            };
            grid.Widgets.Add(displayFrequencyCheck);

            var label8 = new Label
            {
                Text = "Show Idle Lines:",
                GridRow = 7
            };
            grid.Widgets.Add(label8);

            var displayIdleLindesCheck = new CheckBox()
            {
                GridRow = 7,
                GridColumn = 1,
                IsChecked = _config.DisplayConfig.DrawIdleLines
            };
            grid.Widgets.Add(displayIdleLindesCheck);

            var label6 = new Label
            {
                Text = "Background:",
                GridRow = 8
            };
            grid.Widgets.Add(label6);
            var colorButton = new TextButton
            {
                GridRow = 8,
                GridColumn = 1,
                Text = "Color",
                Padding = new Thickness(2),
                TextColor = _config.DisplayConfig.BackgroundColor,
            };
            colorButton.Click += (s, e) =>
            {
                ChooseBackgroundColor(colorButton);
            };
            grid.Widgets.Add(colorButton);

            var label7 = new Label
            {
                Text = "Layout:",
                GridRow = 9,
            };
            grid.Widgets.Add(label7);
            var layoutComboBox = new ComboBox()
            {
                GridRow = 9,
                GridColumn = 1,
            };

            foreach (LayoutStyle value in Enum.GetValues(typeof(LayoutStyle)))
            {
                var item = new ListItem(value.ToString(), Color.White, value);
                layoutComboBox.Items.Add(item);
                if (_config.DisplayConfig.Layout == value)
                {
                    layoutComboBox.SelectedItem = item;
                }
            }
            grid.Widgets.Add(layoutComboBox);

            dialog.Content = grid;
            dialog.Closed += (s, a) =>
            {
                if (!dialog.Result)
                {
                    return;
                }
                if (int.TryParse(displayWidthText.Text, out var displayWidth))
                {
                    _config.DisplayConfig.LineLength = displayWidth < 10 ? 1 : displayWidth;
                }
                if (int.TryParse(pressThresholdText.Text, out var pressThresholdSeconds))
                {
                    _config.DisplayConfig.MinDisplayDuration = pressThresholdSeconds < 1 ? 1 : pressThresholdSeconds;
                }
                if (int.TryParse(frequencyThresholdText.Text, out var frequencyThresholdValue))
                {
                    _config.DisplayConfig.MinDisplayFrequency = frequencyThresholdValue < 1 ? 1 : frequencyThresholdValue;
                }
                _config.DisplayConfig.Speed = displaySpeedSpin.Value;
                _config.DisplayConfig.TurnOffLineSpeed = turnOffLineSpeedSpin.Value * 50.0f;
                _config.DisplayConfig.DisplayDuration = displayDurationCheck.IsChecked;
                _config.DisplayConfig.DisplayFrequency = displayFrequencyCheck.IsChecked;
                _config.DisplayConfig.DrawIdleLines = displayIdleLindesCheck.IsChecked;
                _config.DisplayConfig.BackgroundColor = colorButton.TextColor;
                _config.DisplayConfig.Layout = (LayoutStyle)layoutComboBox.SelectedItem.Tag;

                if( DisplaySettingsUpdated != null )
                {
                    DisplaySettingsUpdated(this, EventArgs.Empty);
                }
            };
            dialog.ShowModal(_desktop);
        }

        private void ChooseColor(GamepadButtonMapping mapping, TextButton colorButton)
        {
            var colorWindow = new ColorPickerDialog();
            colorWindow.Color = colorButton.TextColor;
            colorWindow.ColorPickerPanel._saveColor.Visible = false;
            colorWindow.ShowModal(_desktop);

            colorWindow.Closed += (s, a) =>
            {
                if (!colorWindow.Result)
                {
                    return;
                }
                mapping.Color = colorWindow.Color;
                colorButton.TextColor = colorWindow.Color;
            };
        }

        private void ChooseBackgroundColor(TextButton colorButton)
        {
            var colorWindow = new ColorPickerDialog();
            colorWindow.Color = colorButton.TextColor;
            colorWindow.ShowModal(_desktop);

            colorWindow.Closed += (s, a) =>
            {
                if (!colorWindow.Result)
                {
                    return;
                }
                colorButton.TextColor = colorWindow.Color;
            };
        }
    }
}