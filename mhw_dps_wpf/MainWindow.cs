using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace mhw_dps_wpf {
	public partial class MainWindow : Window, IComponentConnector {

		private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public PlayerList players;
        public MonsterList monsters;

		private int my_seat_id = -5;

		private Rectangle[] damage_bar_rects = new Rectangle[4];
		private TextBlock[] player_name_tbs = new TextBlock[4];
		private TextBlock[] player_dmg_tbs = new TextBlock[4];
        private TextBlock[] player_dps_tbs = new TextBlock[4];

        public LogFile logFile = null;

        private bool questEnded = false;
        

		private static Color[] player_colors = new Color[4]{
			Color.FromRgb(225, 65, 55),
			Color.FromRgb(53, 136, 227),
			Color.FromRgb(196, 172, 44),
			Color.FromRgb(42, 208, 55)
		};

		private bool init_finished;

		private double last_activated = time();

		// internal Canvas front_canvas;

		// private bool _contentLoaded;

		public MainWindow() {
			base.Topmost = true;
			base.AllowsTransparency = true;
			base.WindowStyle = WindowStyle.None;
			base.Background = Brushes.Transparent;
            mhw.initMemory();
            players = new PlayerList(this);
            monsters = new MonsterList(this);
            InitializeComponent();
		}

		public static void assert(bool flag, string reason = "", bool show_reason = true) {
			if (!flag) {
                if (show_reason) {
					MessageBox.Show("assertion failed: " + reason);
				}
                Application.Current.Shutdown();
            }
        }

		private void update_tick(object sender, EventArgs e) {
			if (mhw.hasGameExited()) {
				Application.Current.Shutdown();
			}
			if (init_finished) {
                int playerSeatID = mhw.get_player_seat_id();
                bool isValid = playerSeatID >= 0; 
                if (isValid) {
                    players.update();
                    //for(int i = 0; i < 3; i++) {
                    //    mhw.MonsterInfo info = mhw.getMonsterInfo(i);
                    //    // if(info.maxhp > 0) Console.WriteLine(i + ": " + info.hp + "/" + info.maxhp);
                    //}
                    monsters.update();
					my_seat_id = playerSeatID;
                    if (players[0].isValid) {
                        update_info(my_seat_id < 0);
                        questEnded = false;
                    }
				} else if (playerSeatID == -1 && my_seat_id != -5) {
                    if(!questEnded) {
                        log("Quest ended");
                        log("-----------");
                        logFile.writeBottomAndClose(players);
                        logFile = null;
                        // todo reset player list
                        players = new PlayerList(this);
                    }
                    questEnded = true;
                    update_info(quest_end: true);
				}
			}
		}

		private void update_info(bool quest_end){
		    if (init_finished) {
                int totalDamage = players.totalDamage();
				for (int i = 0; i < 4; i++) {
                    if(!players[i].isValid) {
                        player_name_tbs[i].Visibility = Visibility.Collapsed;
                        player_dmg_tbs[i].Visibility = Visibility.Collapsed;
                        player_dps_tbs[i].Visibility = Visibility.Collapsed;
                        damage_bar_rects[i].Visibility = Visibility.Collapsed;
                        player_dmg_tbs[i].Text = "";
                        player_dps_tbs[i].Text = "";
                    } else {
                        player_name_tbs[i].Visibility = Visibility.Visible;
                        player_dmg_tbs[i].Visibility = Visibility.Visible;
                        player_dps_tbs[i].Visibility = Visibility.Visible;
                        damage_bar_rects[i].Visibility = Visibility.Visible;
                        player_name_tbs[i].Text = players[i].name;
					    player_dmg_tbs[i].Text =  players[i].damage.ToString() + " (" + ((float)players[i].damage / (float)totalDamage * 100f).ToString("0.0") + "%)";
                        player_dps_tbs[i].Text = players[i].getLastDPS(1800).ToString("0.00") + "/s";
                        if (totalDamage == 0) {
                            damage_bar_rects[i].Width = front_canvas.ActualWidth;
                        } else {
                            damage_bar_rects[i].Width = (double)players[i].damage / (double)players.maxDamage() * front_canvas.ActualWidth;
                            if (i == my_seat_id) {
                                damage_bar_rects[i].StrokeThickness = 1.0;
                            } else {
                                damage_bar_rects[i].StrokeThickness = 0.0;
                            }
                        }
                    }
                }
                combatLog.Height = front_canvas.ActualHeight * ( 0.5 + 0.125 * (4 - players.getPlayerNumber()));
            }
		}

		private void update_layout() {
			if (init_finished) {
                double totalHeight = front_canvas.ActualHeight * 0.5;
				double verticalPadding = 0.0;
                double relativeFontSize = 14;
                double relativeLogFontSize = 12;
                double relativeHeight = 0.25;
                double textRelativeVerticalOffset = 0.01;
				double barHeight = (totalHeight - 2 * verticalPadding) * relativeHeight;
                double dpsHorizontalOffset = 0.35 * front_canvas.ActualWidth;
				double verticalPosition = (totalHeight - barHeight - 2 * verticalPadding) / 3;
				for (int i = 0; i < 4; i++) {
					damage_bar_rects[i].Height = barHeight;
                    player_name_tbs[i].Height = barHeight;
                    player_dmg_tbs[i].Height = barHeight;
                    player_dps_tbs[i].Height = barHeight;

                    player_dmg_tbs[i].Width = front_canvas.ActualWidth * 0.7;
                    player_dps_tbs[i].Width = front_canvas.ActualWidth * 0.7;
                    player_name_tbs[i].Width = front_canvas.ActualWidth * 0.7;

                    Canvas.SetTop(damage_bar_rects[i], (double)i * verticalPosition + verticalPadding);
					Canvas.SetTop(player_name_tbs[i], (double)i * verticalPosition + verticalPadding + textRelativeVerticalOffset * totalHeight);
					Canvas.SetTop(player_dmg_tbs[i], (double)i * verticalPosition + verticalPadding + textRelativeVerticalOffset * totalHeight);
                    Canvas.SetTop(player_dps_tbs[i], (double)i * verticalPosition + verticalPadding + textRelativeVerticalOffset * totalHeight);

                    Canvas.SetLeft(player_dmg_tbs[i], front_canvas.ActualWidth - player_dmg_tbs[i].Width);
                    Canvas.SetLeft(player_dps_tbs[i], front_canvas.ActualWidth - player_dps_tbs[i].Width - dpsHorizontalOffset);

                    player_dmg_tbs[i].FontSize = relativeFontSize * barHeight / 20.0;
                    player_name_tbs[i].FontSize = relativeFontSize * barHeight / 20.0;
                    player_dps_tbs[i].FontSize = relativeFontSize * barHeight / 20.0;
                }
				if (players.totalDamage() == 0) {
					for (int i = 0; i < 4; i++) {
						damage_bar_rects[i].Width = 0.0;
					}
				} else {
					for (int i = 0; i < 4; i++) {
						damage_bar_rects[i].Width = (double)players[i].damage / (double)players.maxDamage() * front_canvas.ActualWidth;
						if (i == my_seat_id) {
							damage_bar_rects[i].StrokeThickness = 1.0;
						} else {
							damage_bar_rects[i].StrokeThickness = 0.0;
						}
					}
				}
                combatLog.Height = front_canvas.ActualHeight * (0.5 + 0.125 * (4 - (players.getPlayerNumber()==0 ? 4 : players.getPlayerNumber())));
                combatLog.Width = front_canvas.ActualWidth;
                combatLog.FontSize = relativeLogFontSize * barHeight / 20.0;
            }
		}

		private void init_canvas() {
			init_finished = true;
			//double num = front_canvas.ActualHeight * 0.23000000417232513 - 1.75;
			//double num2 = (front_canvas.ActualHeight - num) / 3.0;
			for (int i = 0; i < 4; i++) {
				damage_bar_rects[i] = new Rectangle();
				damage_bar_rects[i].Stroke = new SolidColorBrush(Colors.White);
				damage_bar_rects[i].StrokeThickness = 0.0;
				damage_bar_rects[i].Fill = new SolidColorBrush(player_colors[i]);
				damage_bar_rects[i].Fill.Opacity = 0.65;
				front_canvas.Children.Add(damage_bar_rects[i]);

                player_name_tbs[i] = new TextBlock();
                player_name_tbs[i].FontWeight = FontWeights.Light;
				player_name_tbs[i].Foreground = new SolidColorBrush(Colors.White);
				player_name_tbs[i].Effect = new DropShadowEffect
				{
					ShadowDepth = 0.0,
					Color = Colors.Black,
					BlurRadius = 1.0,
					Opacity = 1.0
				};
                player_name_tbs[i].FontFamily = new FontFamily("Consolas");
				Canvas.SetLeft(player_name_tbs[i], 3.0);
				front_canvas.Children.Add(player_name_tbs[i]);

                player_dmg_tbs[i] = new TextBlock();
				player_dmg_tbs[i].TextAlignment = TextAlignment.Right;
				player_dmg_tbs[i].Text = i.ToString() + " (-%)";
				player_dmg_tbs[i].Effect = new DropShadowEffect {
					ShadowDepth = 0.0,
					Color = Colors.Black,
					BlurRadius = 1.0,
					Opacity = 1.0
				};
                player_dmg_tbs[i].FontFamily = new FontFamily("Consolas");
                player_dmg_tbs[i].FontWeight = FontWeights.Light;
				player_dmg_tbs[i].Foreground = new SolidColorBrush(Colors.White);
				front_canvas.Children.Add(player_dmg_tbs[i]);

                player_dps_tbs[i] = new TextBlock();
                player_dps_tbs[i].TextAlignment = TextAlignment.Right;
                player_dps_tbs[i].Text = "-/s";
                player_dps_tbs[i].Effect = new DropShadowEffect {
                    ShadowDepth = 0.0,
                    Color = Colors.Black,
                    BlurRadius = 1.0,
                    Opacity = 1.0
                };
                player_dps_tbs[i].FontFamily = new FontFamily("Consolas");
                player_dps_tbs[i].FontWeight = FontWeights.Light;
                player_dps_tbs[i].Foreground = new SolidColorBrush(Colors.White);
                front_canvas.Children.Add(player_dps_tbs[i]);
            }
            player_name_tbs[0].Text = "modified by pv42";//"拖统计条：移动窗口";
			player_name_tbs[1].Text = "orginal by hqvrrsc4";
			player_name_tbs[2].Text = "drag bars to move"; //"滚轮：放大缩小窗口";
			player_name_tbs[3].Text = "mouse wheel to zoom";

            combatLog.FontFamily = new FontFamily("Consolas");
            log("Monster Hunter World Damage Meter");
            log("modified by pv42");
            log("original by hqvrrsc4");
            log("log file version " + LogFile.FILE_VERSION);
            log("---------------------------------");
			update_layout();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			init_canvas();
			dispatcherTimer.Tick += update_tick;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
			dispatcherTimer.Start();
			base.ShowInTaskbar = false;
			base.ShowInTaskbar = true;
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
			update_layout();
		}

		private static double time() {
			return (DateTime.UtcNow - DateTime.MinValue).TotalSeconds;
		}

		private void Window_Activated(object sender, EventArgs e) {
			double num = time() - last_activated;
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
			try {
				if (e.LeftButton == MouseButtonState.Pressed) {
					DragMove();
				}
			} catch (Exception) {
                Console.WriteLine("!! MouseDownException !!");
			}
		}

		private void Window_MouseWheel(object sender, MouseWheelEventArgs e) {
			if (e.RightButton == MouseButtonState.Pressed) {
				double num = base.Width + (double)((float)e.Delta * 0.1f);
				if (num > base.MinWidth)
				{
					base.Width = num;
				}
			} else if (e.LeftButton == MouseButtonState.Pressed) {
				double num2 = base.Height + (double)((float)e.Delta * 0.1f);
				if (num2 > base.MinHeight)
				{
					base.Height = num2;
				}
			} else {
				double num3 = base.Width + (double)((float)e.Delta * 0.07f);
				if (num3 > base.MinWidth) {
					base.Width = num3;
				}
				double num4 = base.Height + (double)((float)e.Delta * 0.03f);
				if (num4 > base.MinHeight) {
					base.Height = num4;
				}
			}
		}

        public void log(string text) {
            if (!combatLog.Text.Equals("")) combatLog.AppendText("\n");
            combatLog.AppendText(text);
            combatLog.ScrollToEnd();
        }

        /*
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				Uri resourceLocator = new Uri("/mhw_dps_wpf;component/mainwindow.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}
        */

        /*
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				((MainWindow)target).Loaded += Window_Loaded;
				((MainWindow)target).SizeChanged += Window_SizeChanged;
				((MainWindow)target).MouseDown += Window_MouseDown;
				((MainWindow)target).MouseWheel += Window_MouseWheel;
				break;
			case 2:
				front_canvas = (Canvas)target;
				break;
			default:
				_contentLoaded = true;
				break;
			}
		}*/
    }
}
