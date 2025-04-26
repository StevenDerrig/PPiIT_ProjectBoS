using BookingApp.Models;
using BookingApp.Services;
using static BookingApp.Services.RoomService;

namespace BookingApp;

public partial class CheckBookingPage : ContentPage
{
    private readonly RoomService _roomService;
    private readonly BookingService _bookingService;
    private readonly APIService _apiService;

    private List<Room> _rooms;
    private List<Booking> _bookings;

    // Calandar status
    private DateTime currentDate;
    private DateTime selectedDate;
    private Dictionary<DateTime, List<Booking>> bookingsByDate;
    private Dictionary<DateTime, int> bookingCountPerDay;

    // UI elements for the calendar
    private List<Border> calendarCells = new List<Border>();
    public CheckBookingPage()
    {
        InitializeComponent();
        var apiService = new APIService();
        _roomService = new RoomService(apiService);
        _bookingService = new BookingService(apiService);

        // Initialize the calendar
        currentDate = DateTime.Now;
        selectedDate = currentDate;

        // Load the calendar and bookings
        LoadCalendar();
        LoadBookingData();
    }

    private async void LoadBookingData()
    {
        try
        {
            // Loaing indicator
            IsBusy = true;

            // Get the rooms and bookings
            _rooms = await _roomService.GetAllRoomsAsync();
            _bookings = await _bookingService.GetAllBookingsAsync();

            // Load and process the bookings by date
            ProcessBookingsData();

            // Update the UI of the calendar
            UpdateCalendarUI();

            // Display the selected date and booking details
            DisplaySelectedDateBookings();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load booking data: {ex.Message}", "OK");
            Console.WriteLine($"Error loading booking data: {ex}");
        }
        finally
        {
            // Hide the loading indicator
            IsBusy = false;
        }
    }

    private void ProcessBookingsData()
    {
        bookingsByDate = new Dictionary<DateTime, List<Booking>>();
        bookingCountPerDay = new Dictionary<DateTime, int>();

        // Group bookings by date
        foreach (var booking in _bookings)
        {
            DateTime currentDate = booking.CheckInDate.Date;

                DateTime bookingDate = currentDate.Date;

                if (!bookingsByDate.ContainsKey(currentDate))
                {
                    bookingsByDate[currentDate] = new List<Booking>();
                    bookingCountPerDay[currentDate] = 0;
                }
                bookingsByDate[currentDate].Add(booking);
                bookingCountPerDay[currentDate]++;
        }
    }

    private void LoadCalendar()
    {
        // Clear existing calendar cells
        DaysGrid.Children.Clear();
        calendarCells.Clear();

        // Set the inital visibility of the calendar
        CalendarScrollView.IsVisible = true;
        DayNavigationGrid.IsVisible = false;

        MonthYearLabel.Text = currentDate.ToString("MMMM yyyy");

        // Set the number of columns in the grid
        foreach (var rowDefinition in DaysGrid.RowDefinitions)
        {
            rowDefinition.Height = 60;
        }

        // Get the first day of the month and the number of days in the month
        DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

        // Get the first day of the week (Sunday = 0, Monday = 1, etc.)
        int firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

        // Adjust the first day of the week to start from Monday
        int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

        // Create calendar cells
        for (int day = 1; day <= daysInMonth; day++)
        {
            DateTime dateOfCell = new DateTime(currentDate.Year, currentDate.Month, day);

            // Calulate the row and column for the cell
            int dayPosition = day + firstDayOfWeek - 1;
            int row = dayPosition / 7;
            int column = dayPosition % 7;

            Console.WriteLine($"Row: {row}, Column: {column}, Day: {day}");

            // Make the calendar cell
            var cellBorder = new Border()
            {
                BackgroundColor = Colors.LightGray,
                StrokeThickness = 1,
                Stroke = Colors.Black,
                Padding = new Thickness(5),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                HeightRequest = 60,
                MinimumHeightRequest = 60
            };

            // Stack layout to hold the date and occupancy indicator
            var cellContent = new VerticalStackLayout();

            var dateLabel = new Label()
            {
                Text = day.ToString(),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14
            };

            // Booking indicator 
            var statusBar = new BoxView()
            {
                HeightRequest = 8,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.End,
                Color = Colors.Transparent,
                Margin = new Thickness(0, 5, 0, 0)
            };

            // Add all the elements to the cell
            cellContent.Children.Add(dateLabel);
            cellContent.Children.Add(statusBar);
            cellBorder.Content = cellContent;
            // Store date in the cell
            cellBorder.BindingContext = dateOfCell;

            // Add tap gesture recognizer to the cell
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnCalendarCellTapped;
            cellBorder.GestureRecognizers.Add(tapGesture);

            DaysGrid.Add(cellBorder, column, row);
            calendarCells.Add(cellBorder);
        }
    }

    private void UpdateCalendarUI()
    {
        int totalRooms = _rooms.Count;

        foreach (var cell in calendarCells)
        {
            if (cell.BindingContext is DateTime cellDate)
            {
                // Check if there are bookings for the date
                var statusBar = ((VerticalStackLayout)cell.Content).Children.Last() as BoxView;

                if (statusBar != null)
                {
                    if (bookingCountPerDay.TryGetValue(cellDate, out int bookingCount))
                    {
                        // Update the status bar color based on occupancy
                        if (bookingCount <= 0)
                        {
                            statusBar.Color = Colors.Black; // No bookings
                        }
                        else if (bookingCount < 3)
                        {
                            statusBar.Color = Colors.Green; // Low occupancy
                        }
                        else if (bookingCount < totalRooms)
                        {
                            statusBar.Color = Colors.Yellow; // Medium occupancy
                        }
                        else
                        {
                            statusBar.Color = Colors.Red; // High occupancy
                        }
                    }
                    else
                    {
                        // No bookings for this date
                        statusBar.Color = Colors.Transparent; // No bookings
                    }
                }

                // Highlight selected date
                if (cellDate.Date == selectedDate.Date)
                {
                    cell.BackgroundColor = Colors.LightBlue;
                }
                else
                {
                    cell.BackgroundColor = Colors.LightGray;
                }
            }
        }
    }

    // Display selected date bookings
    private void DisplaySelectedDateBookings()
    {
        // Update the selected date label
        SelectedDateLabel.Text = selectedDate.ToString("MMMM dd, yyyy");

        // Show the room details to display
        var roomDetails = new VerticalStackLayout {Spacing = 5};

        if (bookingsByDate.TryGetValue(selectedDate, out var dayBooking))
        {
            var bookingsByRoom = dayBooking.GroupBy(b => b.Room.RoomId).ToDictionary(g => g.Key, g => g.ToList());

            // Room display
            foreach (var room in _rooms.OrderBy(r => r.RoomId))
            {
                int guestCount = 0;
                if (bookingsByRoom.TryGetValue(room.RoomId, out var roomBookings))
                {
                    guestCount = roomBookings.Count;
                }

                string guestText = guestCount == 1 ? "guest" : "guests";
                roomDetails.Children.Add(new Label
                {
                    Text = $"{room.RoomNumber}: {guestCount} {guestText}",
                    FontSize = 16
                });
            }
        }
        else
        {
            roomDetails.Children.Add(new Label
            {
                Text = "No bookings for this date.",
                FontSize = 16
            });
        }

        SelectedDayDetails.Children.Clear();
        SelectedDayDetails.Children.Add(SelectedDateLabel);
        SelectedDayDetails.Children.Add(new Label
        {
            Text = "Rooms",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold
        });
        SelectedDayDetails.Children.Add(roomDetails);

        // Edit button
        var editButton = new Button
        {
            Text = "✏️",
            BackgroundColor = Color.Parse("#8F4A56"),
            TextColor = Colors.White,
            CornerRadius = 25,
            HeightRequest = 50,
            WidthRequest = 50,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, -50, 0, 0)
        };
        editButton.Clicked += OnEditBookingsClicked;
        SelectedDayDetails.Children.Add(editButton);

        // Change the layout from the calendar to the selected date
        if (!DayNavigationGrid.IsVisible && !CalendarGrid.IsVisible)
        {
            DayNavigationGrid.IsVisible = true;
        }
    }

    private void OnCalendarCellTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is DateTime cellDate)
        {
            selectedDate = cellDate;

            // Switch to day navigation view
            CalendarScrollView.IsVisible = false;
            DayNavigationGrid.IsVisible = true;

            //UpdateCalendarUI();
            DisplaySelectedDateBookings();
        }
    }

    // Prevoius month button
    private void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        currentDate = currentDate.AddMonths(-1);
        LoadCalendar();
        LoadBookingData();
    }

    // Next month button
    private void OnNextMonthClicked(object sender, EventArgs e)
    {
        currentDate = currentDate.AddMonths(1);
        LoadCalendar();
        LoadBookingData();
    }

    // Previous day button
    private void OnPreviousDayClicked(object sender, EventArgs e)
    {
        selectedDate = selectedDate.AddDays(-1);
        DisplaySelectedDateBookings();
    }

    // Next day button
    private void OnNextDayClicked(object sender, EventArgs e)
    {
        selectedDate = selectedDate.AddDays(1);
        DisplaySelectedDateBookings();
    }

    // Back to calendar view button
    private void OnCalendarViewClicked(object sender, EventArgs e)
    {
        // Switch back to calendar view
        CalendarScrollView.IsVisible = true;
        DayNavigationGrid.IsVisible = false;

        // Update calendar UI to highlight the selected date
        UpdateCalendarUI();
    }

    // Edit booking button
    private async void OnEditBookingsClicked(object sender, EventArgs e)
    {
        var dayBookings = _bookings.Where(b =>
        b.CheckInDate.Date <= selectedDate.Date &&
        selectedDate.Date < b.CheckOutDate.Date).ToList();

        // Check if we have bookings for this date
        if (dayBookings.Any())
        {
            // Forward to the update booking page with these bookings
            await Navigation.PushAsync(new UpdateBookingPage(selectedDate, dayBookings));
        }
        else
        {
            await DisplayAlert("No Bookings", "There are no bookings to edit for this date.", "OK");
        }
    }
}