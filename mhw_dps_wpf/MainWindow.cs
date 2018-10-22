using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace mhw_dps_wpf
{
	public partial class MainWindow : Window, IComponentConnector
	{
		private static byte?[] pattern_1 = new byte?[26]
		{
			(byte)139,
			(byte)13,
			null,
			null,
			null,
			null,
			(byte)35,
			(byte)202,
			(byte)129,
			(byte)249,
			0,
			(byte)1,
			0,
			0,
			(byte)115,
			(byte)47,
			(byte)15,
			(byte)183,
			null,
			null,
			null,
			null,
			null,
			(byte)193,
			(byte)234,
			(byte)16
		};

		private static byte?[] pattern_2 = new byte?[58]
		{
			(byte)72,
			(byte)137,
			(byte)116,
			(byte)36,
			(byte)56,
			(byte)139,
			(byte)112,
			(byte)24,
			(byte)72,
			(byte)139,
			null,
			null,
			null,
			null,
			null,
			(byte)137,
			(byte)136,
			(byte)12,
			(byte)5,
			0,
			0,
			(byte)72,
			(byte)139,
			null,
			null,
			null,
			null,
			null,
			(byte)137,
			(byte)144,
			(byte)16,
			(byte)5,
			0,
			0,
			(byte)72,
			(byte)139,
			null,
			null,
			null,
			null,
			null,
			(byte)137,
			(byte)152,
			(byte)20,
			(byte)5,
			0,
			0,
			(byte)133,
			(byte)219,
			(byte)126,
			null,
			(byte)72,
			(byte)139,
			null,
			null,
			null,
			null,
			null
		};

		private static byte?[] pattern_3 = new byte?[21]
		{
			(byte)178,
			(byte)172,
			(byte)11,
			0,
			0,
			(byte)73,
			(byte)139,
			(byte)217,
			(byte)139,
			(byte)81,
			(byte)84,
			(byte)73,
			(byte)139,
			(byte)248,
			(byte)72,
			(byte)139,
			(byte)13,
			null,
			null,
			null,
			null
		};

		private static byte?[] pattern_4 = new byte?[37]
		{
			(byte)72,
			(byte)139,
			(byte)13,
			null,
			null,
			null,
			null,
			(byte)72,
			(byte)141,
			(byte)84,
			(byte)36,
			(byte)56,
			(byte)198,
			(byte)68,
			(byte)36,
			(byte)32,
			0,
			(byte)77,
			(byte)139,
			(byte)64,
			(byte)8,
			(byte)232,
			null,
			null,
			null,
			null,
			(byte)72,
			(byte)139,
			(byte)92,
			(byte)36,
			(byte)96,
			(byte)72,
			(byte)131,
			(byte)196,
			(byte)80,
			(byte)95,
			(byte)195
		};

		private DispatcherTimer dispatcherTimer = new DispatcherTimer();

		private Process game;

        private PlayerList players;

		private int my_seat_id = -5;

		private Rectangle[] damage_bar_rects = new Rectangle[4];

		private TextBlock[] player_name_tbs = new TextBlock[4];

		private TextBlock[] player_dmg_tbs = new TextBlock[4];

        private TextBlock[] player_dps_tbs = new TextBlock[4];

        private LogFile logFile;

        private bool questEnded = false;

        

		private static Color[] player_colors = new Color[4]
		{
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
			find_game_proc();
			mhw.is_wegame_build = game.MainWindowTitle.Contains("怪物猎人 世界"); // Monster hunter world
            bool flag = game.MainWindowTitle.Contains("3142");
			if (!mhw.is_wegame_build) {
				Console.WriteLine("main module base adress 0x" + game.MainModule.BaseAddress.ToString("X"));
				List<byte?[]> patterns = new List<byte?[]> {
					pattern_1,
					pattern_2,
					pattern_3,
					pattern_4
				};
				ulong[] array = memory.find_patterns(game, (IntPtr)5368725504L, (IntPtr)5452595200L, patterns);
				assert(array[0] > 5369757695L && array[1] > 5369757695L && array[1] > 5369757695L && array[3] > 5369757695L, "failed to locate offsets (step 1).");
				ulong num = array[0] + mhw.read_uint(game.Handle, (IntPtr)(long)(array[0] + 2)) + 6;
				ulong num2 = array[1] + 51 + mhw.read_uint(game.Handle, (IntPtr)(long)(array[1] + 54)) + 7;
				ulong num3 = array[2] + 15 + mhw.read_uint(game.Handle, (IntPtr)(long)(array[2] + 15 + 2)) + 6;
				ulong num4 = array[3] + mhw.read_uint(game.Handle, (IntPtr)(long)(array[3] + 3)) + 7;
				Console.WriteLine("0x" + num.ToString("X"));
				Console.WriteLine("dmg base adress 0 0x" + num2.ToString("X"));
				Console.WriteLine("dmg base adress 1 0x" + num3.ToString("X"));
				Console.WriteLine("names base adress 0x" + num4.ToString("X"));
				assert(num > 5368725504L && num < 5637144576L && num2 > 5368725504L && num2 < 5637144576L && num3 > 5368725504L && num3 < 5637144576L && num4 > 5368725504L && num4 < 5637144576L, "failed to locate offsets (step 2).");
				mhw.loc1 = (long)num;
				mhw.damage_base_loc0 = (long)num2;
				mhw.damage_base_loc1 = (long)num3;
				mhw.names_base_adress = (long)num4;
			} else {
				assert(flag, "版本错误，必须为3142才能使用"); //The version is wrong and must be 3142 to use
                mhw.loc1 = 5428988018L;
				mhw.damage_base_loc0 = 5430764760L;
				mhw.damage_base_loc1 = 5430775464L;
				mhw.names_base_adress = 5444356280L;
			}
            players = new PlayerList(this);
            InitializeComponent();
		}

		private static void assert(bool flag, string reason = "", bool show_reason = true) {
			if (!flag) {
                Application.Current.Shutdown();
                if (show_reason) {
					MessageBox.Show("assertion failed: " + reason);
				}
			}
		}

		private void find_game_proc() {
			IEnumerable<Process> source = from x in Process.GetProcesses()
			where x.ProcessName == "MonsterHunterWorld"
			select x;
			assert(source.Count() == 1, "frm_main_load: #proc not 1. (Is the game running?)");
			game = source.FirstOrDefault();
			try {
				Console.WriteLine("Game base adress 0x" + game.MainModule.BaseAddress.ToString("X"));
			} catch (Exception) {
				assert(flag: false, "access denied. (Is the game running as admin while the tool isn't? ) WEGAME版必须以管理员身份运行");
			}
		}

		private void update_tick(object sender, EventArgs e) {
			if (game.HasExited) {
				Application.Current.Shutdown();
			}
			if (init_finished) {
                int[][] data  = mhw.get_team_data(game);
                int[] playerDamages = data[(int)data_indices.damages];
                int[] playerSlingers = data[(int)data_indices.slingers];
                int[] playerTracks = data[(int)data_indices.tracks];
                int[] playerLocates = data[(int)data_indices.located];
                int[] playerPartsBroken = data[(int)data_indices.parts];
                string[] playerNames = mhw.get_team_player_names(game);
				int playerSeatID = mhw.get_player_seat_id(game);
                bool isValid = playerDamages.Sum() != 0 && playerSeatID >= 0 && playerNames[0] != "";
				bool flag2 = false;
				for (int i = 0; i < 4; i++) {
					flag2 |= (players[i].damage != playerDamages[i] && playerDamages[i] > 0);
                    flag2 |= (players[i].slingers != playerSlingers[i] && playerSlingers[i] > 0);
                    flag2 |= (players[i].parts_broken != playerPartsBroken[i] && playerPartsBroken[i] > 0);
                    flag2 |= (players[i].tracks_collected != playerTracks[i] && playerTracks[i] > 0);
                    flag2 |= (players[i].monsters_located != playerLocates[i] && playerLocates[i] > 0);
                    flag2 |= (players[i].name != playerNames[i] && playerNames[i] != "");
				}
                if (isValid && flag2) {
                    for (int i = 0; i < 4; i++) {
                        players[i].name = playerNames[i];
                        players[i].damage = playerDamages[i];
                        players[i].slingers = playerSlingers[i];
                        players[i].parts_broken = playerPartsBroken[i];
                        players[i].monsters_located = playerLocates[i];
                        players[i].tracks_collected = playerTracks[i];
                    }
					my_seat_id = playerSeatID;
					update_info(my_seat_id < 0);
                    questEnded = false;
				} else if (playerSeatID == -1 && my_seat_id != -5) {
                    if(!questEnded) {
                        log("Quest ended");
                        log("-----------");
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
                    if(players[i].name == "") {
                        player_name_tbs[i].Visibility = Visibility.Collapsed;
                        player_dmg_tbs[i].Visibility = Visibility.Collapsed;
                        player_dps_tbs[i].Visibility = Visibility.Collapsed;
                        damage_bar_rects[i].Visibility = Visibility.Collapsed;
                    } else {
                        player_name_tbs[i].Visibility = Visibility.Visible;
                        player_dmg_tbs[i].Visibility = Visibility.Visible;
                        player_dps_tbs[i].Visibility = Visibility.Visible;
                        damage_bar_rects[i].Visibility = Visibility.Visible;
                        player_name_tbs[i].Text = players[i].name;
					    player_dmg_tbs[i].Text = ((players[i].name == "") ? "" : (players[i].damage.ToString() + " (" + ((float)players[i].damage / (float)totalDamage * 100f).ToString("0.0") + "%)"));
                        player_dps_tbs[i].Text = players[i].getLastDPS(1800).ToString("0.00") + "/s";
                        if (totalDamage == 0) {
                            damage_bar_rects[i].Width = 0.0;
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

		private void init_canvas()
		{
			init_finished = true;
			//double num = front_canvas.ActualHeight * 0.23000000417232513 - 1.75;
			//double num2 = (front_canvas.ActualHeight - num) / 3.0;
			for (int i = 0; i < 4; i++)
			{
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
            log("---------------------------------");
			update_layout();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
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
