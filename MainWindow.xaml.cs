using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;
using System.Windows.Media.Animation;
using WMPLib;
namespace SantaShooter
{

    public partial class MainWindow : Window
    {
        public static class Images
        {
            public static readonly BitmapImage santa;
            public static readonly BitmapImage saw;
            public static readonly BitmapImage hearth;

            static Images()
            {
                santa = new BitmapImage(new Uri(Environment.CurrentDirectory + @"/Santa.png"));
                saw = new BitmapImage(new Uri(Environment.CurrentDirectory + @"/Saw.png"));
                hearth = new BitmapImage(new Uri(Environment.CurrentDirectory + @"/Hearth.svg"));

            }
        }

        public DispatcherTimer Spawner;
        public Random rng;
        private int score =0;
        private int hp = 3;
        int m1 = 0;
        int m2 = 0;
        private delegate void voidHandler();
        event voidHandler GameOver;
        private bool immortality = false;
        private bool IsGameOver = false;
        public int Score
        { 
            get => score;
            set
            {
                score = value;
                ScoreLabel.Content = score;
                if (score >= 100)
                    Win();
            }
        }
        public int Hp 
        {
            get
            {
                return hp;
            }
            set
            {
                if (value <= 5 && hp > 0)
                {
                    hp = value;
                    UpdateHealth();
                }
                if (value == 0 && !immortality)
                {
                    Loss();
                }
            }
        }
        MediaPlayer NoisePlayer = new MediaPlayer();
        MediaPlayer musicPlayer = new MediaPlayer();
        public MainWindow()
        {
            InitializeComponent();
            Mouse.OverrideCursor = new Cursor(Environment.CurrentDirectory + @"/Snipe.cur");
            musicPlayer.Open(new Uri(@"../../Sounds/JingleBells.mp3", UriKind.Relative));
            musicPlayer.MediaEnded += (a, b) => { musicPlayer.Position = TimeSpan.Zero; musicPlayer.Play(); };
            musicPlayer.Play();
            ImmortalityCheckBox.Visibility = Visibility.Hidden;
            Spawner = new DispatcherTimer();
            rng = new Random();
            Spawner.Interval = TimeSpan.FromMilliseconds(500);
            Spawner.Tick += (a,b) => SpawnAThing();
            SpawnAThing();
            HP1.Source = HP2.Source = HP3.Source = HP4.Source = HP5.Source = Images.hearth;
            UpdateHealth();
        }

        public abstract class FallingStuff
        {
            public static readonly int size = 150;

            protected MainWindow wnd = (MainWindow)Application.Current.MainWindow;
            public Image img;
            public int speed;
            public PathFigure pathFigure;
            public FallingStuff(int start, BitmapImage _img)
            {
                pathFigure = new PathFigure();
                img = new Image
                {
                    Source = _img,
                    Width = size,
                    Height = size,
                    Margin = new Thickness(start, -size, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                };
                wnd.Griddy.Children.Add(img);

            }
            public virtual void Click()
            {
                Kill();
            }
            public virtual void FallOffOfTheFlatWorld()
            {
                Kill();
            }
            public void Kill()
            {
                wnd.Griddy.Children.Remove(this.img);
            }
            public void PlaySound(string nameAndFormat)
            {



                wnd.NoisePlayer.Stop();
                wnd.NoisePlayer.Open(new Uri(@"../../Sounds/" + nameAndFormat, UriKind.Relative));
                wnd.NoisePlayer.Play();

            
            }
        }
        public class Santa : FallingStuff
        {
            public Santa(int start) : base(start, Images.santa)
            {
                Grid.SetZIndex(img, 1);
                speed = 5;
                pathFigure.StartPoint = new Point(0, -FallingStuff.size / 2);
                PolyBezierSegment segment = new PolyBezierSegment();
                segment.Points.Add(pathFigure.StartPoint);
                for (int i = 1; i <= (wnd.Score / 10) + 3; i++)
                {
                    segment.Points.Add(
                        new Point(
                            wnd.rng.Next(-FallingStuff.size/2, (int)wnd.Griddy.Width - start - FallingStuff.size - (int)segment.Points.Last().X), 
                            wnd.rng.Next((int)segment.Points.Last().Y, i * (((int)wnd.Griddy.Height - FallingStuff.size) / ((wnd.Score / 10) + 3)))
                            ));
                }
                PolyLineSegment polyLineSegment = new PolyLineSegment();
                polyLineSegment.Points.Add(
                    new Point(
                        wnd.rng.Next(-start-FallingStuff.size, (int)wnd.Griddy.Width - start - FallingStuff.size - (int)segment.Points.Last().X), 
                        wnd.Griddy.Height
                        ));
                pathFigure.Segments.Add(segment);
                pathFigure.Segments.Add(polyLineSegment);
            }
            public override void Click()
            {
                PlaySound($@"Snipe/{(++wnd.m1 % 8) +1}.mp3");
                wnd.Score++;
                Kill();
            }
            public override void FallOffOfTheFlatWorld()
            {
                if (!wnd.IsGameOver)
                {
                    wnd.Hp--;
                }
                Kill();
            }
        }
        public class Saw : FallingStuff
        {
            public Saw(int start) : base(start,Images.saw)
            {
                Grid.SetZIndex(img, 2);
                speed = 5;
                pathFigure.StartPoint = new Point(0, -FallingStuff.size / 2);
                PolyBezierSegment segment = new PolyBezierSegment();
                segment.Points.Add(pathFigure.StartPoint);
                for (int i = 1; i <= (wnd.Score / 10) + 6; i++)
                {
                    segment.Points.Add(
                        new Point(
                            wnd.rng.Next(-FallingStuff.size/2, (int)wnd.Griddy.Width - start - FallingStuff.size),
                            wnd.rng.Next(-FallingStuff.size/2, (int)wnd.Griddy.Height - FallingStuff.size)
                            ));
                }
                PolyLineSegment polyLineSegment = new PolyLineSegment();
                polyLineSegment.Points.Add(
                    new Point(
                        wnd.rng.Next(-start-FallingStuff.size, (int)wnd.Griddy.Width - start - FallingStuff.size - (int)segment.Points.Last().X),
                        wnd.Griddy.Height
                        ));
                pathFigure.Segments.Add(segment);
                pathFigure.Segments.Add(polyLineSegment);
            }
            public override void Click()
            {
                int i = (++wnd.m2 % 6) + 1;
                if (i >8 || i <1)
                {
                    throw new Exception();
                }
                PlaySound($@"Punch/{(++wnd.m2 % 6) + 1}.mp3");
                wnd.Hp--;
            }
        }
        public class Hearth : FallingStuff
        {
            public Hearth(int start) : base(start, Images.hearth)
            {
                Grid.SetZIndex(img, 1);
                speed = 3;
                pathFigure.StartPoint = new Point(0, -FallingStuff.size / 2);
                PolyLineSegment segment = new PolyLineSegment();
                segment.Points.Add(pathFigure.StartPoint);
                for (int i = 1; i <= (wnd.Score / 10) + 2; i++)
                {
                    segment.Points.Add(
                        new Point(
                            wnd.rng.Next(-start + FallingStuff.size/2, (int)wnd.Griddy.Width - start - FallingStuff.size), 
                            wnd.rng.Next((int)segment.Points.Last().Y, i * (((int)wnd.Griddy.Height - FallingStuff.size) / ((wnd.Score / 10) + 2)))
                            ));
                }
                PolyLineSegment polyLineSegment = new PolyLineSegment();
                segment.Points.Add(
                    new Point(
                        wnd.rng.Next(-start-FallingStuff.size, (int)wnd.Griddy.Width - start - FallingStuff.size - (int)segment.Points.Last().X), 
                        wnd.Griddy.Height
                        ));
                pathFigure.Segments.Add(segment);
                pathFigure.Segments.Add(polyLineSegment);
            }
            public override void Click()
            {
                //PlaySound("Choir.mp3");
                wnd.Hp++;
                Kill();
            }
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Spawner.Start();
        }
        void UpdateHealth()
        {
            if (Hp >=1)
            {
                if (HP1.Visibility != Visibility.Visible)
                    HP1.Visibility = Visibility.Visible;
            }
            else
            {
                if (HP1.Visibility != Visibility.Hidden)
                    HP1.Visibility = Visibility.Hidden;
            }
            if (Hp >= 2)
            {
                if (HP2.Visibility != Visibility.Visible)
                    HP2.Visibility = Visibility.Visible;
            }
            else
            {
                if (HP2.Visibility != Visibility.Hidden)
                    HP2.Visibility = Visibility.Hidden;
            }
            if (Hp >= 3)
            {
                if (HP3.Visibility != Visibility.Visible)
                    HP3.Visibility = Visibility.Visible;
            }
            else
            {
                if (HP3.Visibility != Visibility.Hidden)
                    HP3.Visibility = Visibility.Hidden;
            }
            if (Hp >= 4)
            {
                if (HP4.Visibility != Visibility.Visible)
                    HP4.Visibility = Visibility.Visible;
            }
            else
            {
                if (HP4.Visibility != Visibility.Hidden)
                    HP4.Visibility = Visibility.Hidden;
            }
            if (Hp >= 5)
            {
                if (HP5.Visibility != Visibility.Visible)
                    HP5.Visibility = Visibility.Visible;
            }
            else
            {
                if (HP5.Visibility != Visibility.Hidden)
                    HP5.Visibility = Visibility.Hidden;
            }
        }
        void Loss()
        {
            IsGameOver = true;
            MessageBox.Show("You Lost" + Environment.NewLine + $"Score: {Score}");
            NewGame();
            
        }
        void Win()
        {
            IsGameOver = true;
            MessageBox.Show("YOU WON");
            NewGame();
        }
        void NewGame()
        {
            GameOver.Invoke();
            Griddy.Children.Clear();
            Score = 0;
            hp = 3;
            UpdateHealth();
            IsGameOver = false;
        }
        
        void SpawnAThing()
        {
           //Spawner.Interval = TimeSpan.FromMilliseconds(Spawner.Interval.Milliseconds * 0.995);
            FallingStuff thing;
            int start = rng.Next(0, (int)Griddy.Width - FallingStuff.size);
            int spawnChance = rng.Next(1,11);
            if (spawnChance <6)
            {
                thing = new Santa(start);
            }
            else if (spawnChance <10)
            {
                thing = new Saw(start);
            }
            else 
            {
                thing = new Hearth(start);
            }
            NameScope.SetNameScope(this, new NameScope());
            thing.img.RenderTransform = new MatrixTransform();
            RegisterName("MatrixTransform", thing.img.RenderTransform);
            PathGeometry animationPath = new PathGeometry();
            animationPath.Figures.Add(thing.pathFigure);

            animationPath.Freeze();

            MatrixAnimationUsingPath anim = new MatrixAnimationUsingPath
            {
                PathGeometry = animationPath,
                Duration = TimeSpan.FromSeconds(thing.speed),
                RepeatBehavior = new RepeatBehavior(1),
                FillBehavior = FillBehavior.Stop
            };
            Storyboard.SetTargetName(anim, "MatrixTransform");
            Storyboard.SetTargetProperty(anim, new PropertyPath(MatrixTransform.MatrixProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anim);
            storyboard.RepeatBehavior = new RepeatBehavior(1);
            thing.img.Loaded += (a, b) => storyboard.Begin(this);
            EventHandler hop = new EventHandler ((a, b) => thing.FallOffOfTheFlatWorld());
            storyboard.Completed += hop;
            GameOver += () => storyboard.Completed -= hop;
            thing.img.MouseDown += (a, b) => { thing.Click(); storyboard.Pause(); storyboard.Completed -= hop; GameOver -= () => storyboard.Completed -= hop; };
        }

        private void ImmortalityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            immortality = true;
        }

        private void ImmortalityCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            immortality = false;
        }
    }
}
