using System.Diagnostics;
using Microsoft.Maui.Controls.Shapes;
using Switch = Microsoft.Maui.Controls.Switch;

namespace SenzoryVelkyTest;

public partial class MainPage : ContentPage
{
    private static int speed = 25;
    private static int orientation = 1;
    private static int sebrano = 0;
    private static int sideLength = 100;
    private static PanGestureRecognizer gestureRecognizer = new();
    private static double panX = 0;
    private static double panY = 0;


    public MainPage()
    {
        gestureRecognizer.PanUpdated += OnPanUpdated;
        
        InitializeComponent();

        ToggleAccelerometer();
        generateCoins(15);
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                // Ukládání aktuální polohy čtverce
                panX = AbsoluteLayout.GetLayoutBounds(rect).X;
                panY = AbsoluteLayout.GetLayoutBounds(rect).Y;
                break;

            case GestureStatus.Running:
                // Přičítání posunu k původní poloze
                var newX = panX + e.TotalX;
                var newY = panY + e.TotalY;
                AbsoluteLayout.SetLayoutBounds(rect, new(newX, newY, sideLength, sideLength));
                pickCoin();
                break;
        }
    }

    //Díky použití akcelerometru se čtverec pohybuje podle zrychlení
    public void ToggleAccelerometer()
    {
        if (Accelerometer.Default.IsSupported)
        {
            if (!Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
            else
            {
                Accelerometer.Default.Stop();
                Accelerometer.Default.ReadingChanged -= Accelerometer_ReadingChanged;
            }
        }
    }
    
    private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        //Pokud je x > 0 tak měšec padá dolů a naopak
        orientation = e.Reading.Acceleration.X > 0 ? 1 : -1; 

        //Z je 1 když je zařízení vodorovně položené, tak se odečítá od 1 aby se pohybovalo když je zařízení nakloněné
        AbsoluteLayout.SetLayoutBounds(
            rect,
            new( 
                rect.X + e.Reading.Acceleration.Y * speed , 
                rect.Y + (1 - e.Reading.Acceleration.Z) * orientation * speed,
            sideLength,sideLength));
        pickCoin();
    }
    private static bool CheckCollision(View rect, View? view)
    {
        if (view is null)
        {
            return false;
        }
        bool collisionX = rect.X + rect.Width >= view.X && view.X + view.Width >= rect.X;
        bool collisionY = rect.Y + rect.Height >= view.Y && view.Y + view.Height >= rect.Y;

        return collisionX && collisionY;
    }

    private void pickCoin()
    {
        for (var i = 0; i < MainLayout.Children.Count; i++)
        {
            var view = MainLayout.Children[i];
            if (view is not Image coin) continue;
            if (!CheckCollision(rect, coin)) continue;

            MainLayout.Remove(coin);
            sebrano++;
            lbl.Text = $"Sebráno: {sebrano}";
            if (sebrano % 15 == 0 && sebrano != 0)
            {
                refreshBtn.IsEnabled = true;
                refreshBtn.BackgroundColor = Colors.MediumPurple;
            }
        }
    }

    private void generateCoins(int number)
    {
        refreshBtn.IsEnabled = false;
        refreshBtn.BackgroundColor = Colors.DarkGray;

        var diameter = 50;
        Random random = new();

        for (var i = 0; i < number; i++)
        {
            var coin = new Image
            {
                WidthRequest = diameter,
                HeightRequest = diameter,
                Source = "coin.png",
            };
            MainLayout.Add(coin);
            //Mince se generují v náhodných pozicích na obrazovce
            //Nepoužilo se DeviceDisplay protože se mince z nějakého důvodu generovali mimo obrazovku
            AbsoluteLayout.SetLayoutBounds(coin,new(random.Next(70, 1000), random.Next(20, 700),diameter,diameter));
        }
    }
    
    private void RefreshBtn_OnClicked(object? sender, EventArgs e)
    {
        generateCoins(15);
    }

    private void ControlsSwitch_OnToggled(object? sender, ToggledEventArgs e)
    {
        ToggleAccelerometer();
        var controlsSwitch = sender as Switch;

        if (controlsSwitch.IsToggled)
        {
            MainLayout.GestureRecognizers.Add(gestureRecognizer);
            controlsLabel.Text = "Ovládání prstem";
        }
        else
        {
            MainLayout.GestureRecognizers.Remove(gestureRecognizer);
            controlsLabel.Text = "Ovládání nakláněním";
        }
    }
}