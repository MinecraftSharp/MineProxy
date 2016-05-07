using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MineProxy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region colors
        //*
        private static readonly Dictionary<string, SolidColorBrush> Colours = new Dictionary<string, SolidColorBrush>
        {
             { "0", Brushes.Black       },
             { "1", Brushes.DarkBlue    },
             { "2", Brushes.DarkGreen   },
             { "3", Brushes.DarkCyan    },
             { "4", Brushes.DarkRed     },
             { "5", Brushes.DarkMagenta },
             { "6", Brushes.Yellow      },
             { "7", Brushes.Gray        },
             { "8", Brushes.DarkGray    },
             { "9", Brushes.Blue        },
             { "a", Brushes.Green       },
             { "b", Brushes.Cyan        },
             { "c", Brushes.Red         },
             { "d", Brushes.Magenta     },
             { "e", Brushes.Yellow      },
             { "f", Brushes.White       },
             { "r", Brushes.White       },

             { "black", Brushes.Black              },
             { "dark_blue", Brushes.DarkBlue       },
             { "dark_green", Brushes.DarkGreen     },
             { "dark_cyan", Brushes.DarkCyan       },
             { "dark_red", Brushes.DarkRed         },
             { "dark_magenta", Brushes.DarkMagenta },
             { "dark_gray", Brushes.DarkGray       },
             { "yellow", Brushes.Yellow            },
             { "gray", Brushes.Gray                },
             { "blue", Brushes.Blue                },
             { "green", Brushes.Green              },
             { "cyan", Brushes.Cyan                },
             { "red", Brushes.Red                  },
             { "magenta", Brushes.Magenta          },
             { "white", Brushes.White              },
             { "gold", Brushes.Gold                }
        };
        //*/
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var serverInfo = serverAddress.Text.Split(':');
            string serverIP;
            int serverPort;
            if (serverInfo.Count() == 1)
            {
                serverIP = serverInfo[0];
                serverPort = 25565;
            }
            else
            {
                serverIP = serverInfo[0];
                try
                {

                    serverPort = Convert.ToInt32(serverInfo[1]);
                }
                catch
                {
                    MessageBox.Show("Error: invalid port, must be an int (25565)", 
                        "Connect Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var ping = new MinecraftPing(serverIP, serverPort);
            ping.OnPingReceived += (o) =>
            {
                //Handle motd
                motdTextbox.Document = new FlowDocument {TextAlignment = TextAlignment.Center};
                motdTextbox.Background = Brushes.DarkGray;
                if (o.description.extra == null || !o.description.extra.Any())
                {
                    motdTextbox.AppendText(o.description.text);
                }
                else
                {
                    foreach (var textInfo in o.description.extra)
                    {
                        try
                        {
                            var text = new TextRange(motdTextbox.Document.ContentEnd, motdTextbox.Document.ContentEnd)
                            {
                                Text = textInfo.text
                            };
                            text.ApplyPropertyValue(TextElement.ForegroundProperty, Colours[textInfo.color]);
                        }
                        catch
                        {
                            //ignored
                        }
                    }
                }

                //Handle server
                var serverThread = new Thread(() =>
                {
                    var proxy = new MinecraftProxy(serverIP, serverPort);
                });
                serverThread.Start();
            };
            ping.OnError += (o) =>
            {
                MessageBox.Show(o, "Ping Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            ping.Ping();
        }
    }
}
