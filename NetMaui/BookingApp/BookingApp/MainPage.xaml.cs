using System;
using Microsoft.Maui.Controls;

namespace BookingApp
{
    public partial class MainPage : ContentPage
    {
        private IDispatcherTimer _timer;

        public MainPage()
        {
            InitializeComponent();
            UpdateTimeAndDate();

            // Set up timer to update the time every minute
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += (s, e) => UpdateTimeAndDate();
            _timer.Start();
        }

        private void UpdateTimeAndDate()
        {
            var now = DateTime.Now;
            TimeLabel.Text = $"Time: {now:HH:mm}";
            DateLabel.Text = $"Date: {now:dd/MM/yyyy}";
            DayLabel.Text = $"Day: {now:dddd}";
        }

        private async void OnCheckBookBtn(object sender, EventArgs e)
        {
            // Navigate to Check Bookings Page
            await DisplayAlert("Navigation", "Navigating to Check Bookings", "OK");
            await Navigation.PushAsync(new CheckBookingPage1());
        }

        private async void OnMakeABookBtn(object sender, EventArgs e)
        {
            // Navigate to Make a Booking Page
            await DisplayAlert("Navigation", "Navigating to Make a Booking", "OK");
            // Actual navigation code would be:
            // await Navigation.PushAsync(new MakeBookingPage());
        }

        private async void OnUpdateBookBtn(object sender, EventArgs e)
        {
            // Navigate to Update Booking Page
            await DisplayAlert("Navigation", "Navigating to Update Booking", "OK");
            // Actual navigation code would be:
            // await Navigation.PushAsync(new UpdateBookingPage());
        }

        private async void OnSettingsBtn(object sender, EventArgs e)
        {
            // Navigate to Settings Page
            await DisplayAlert("Navigation", "Navigating to Settings", "OK");
            // Actual navigation code would be:
            // await Navigation.PushAsync(new SettingsPage());
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Stop timer when page disappears
            if (_timer != null)
            {
                _timer.Stop();
            }
        }
    }
}