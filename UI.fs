namespace CinemaApp

open System
open System.Windows.Forms
open System.Drawing
open Domain

module UI =


    type SeatButton(r, c, onClick: int -> int -> unit) as this =
        inherit Button()
        do
            this.Text <- sprintf "%d-%d" (r+1) (c+1)
            this.Size <- Size(50, 50)
            this.Margin <- Padding(5)
            this.BackColor <- Color.LightGray
            this.Click.Add(fun _ -> onClick r c)
        
        member this.Render(bookedSeats: Set<int*int>) =
            if bookedSeats.Contains((r, c)) then 
                this.BackColor <- Color.Salmon
                this.Enabled <- false
            else 
                this.BackColor <- Color.LightGreen
                this.Enabled <- true

    let createAdminForm (onDataChanged: unit -> unit) =
        let form = new Form(Text = "Admin Dashboard", Size = Size(350, 500), StartPosition = FormStartPosition.CenterParent)
        let tabs = new TabControl(Dock = DockStyle.Fill)
        

        let mutable cachedHalls : Hall list = [] 

        let txtTitle = new TextBox(PlaceholderText = "Movie Title", Dock = DockStyle.Top)
        let txtTime = new TextBox(PlaceholderText = "Time (e.g. 18:00)", Dock = DockStyle.Top)
        let numPrice = new NumericUpDown(Minimum = 1M, Maximum = 100M, Value = 12M, Dock = DockStyle.Top)
        let cmbHalls = new ComboBox(Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList)
        let btnSaveMovie = new Button(Text = "SAVE MOVIE", Dock = DockStyle.Top, Height = 40, BackColor = Color.LightBlue)

        let txtHallName = new TextBox(PlaceholderText = "Hall Name", Dock = DockStyle.Top)
        let numRows = new NumericUpDown(Minimum = 2M, Maximum = 50M, Value = 10M, Dock = DockStyle.Top)
        let numCols = new NumericUpDown(Minimum = 2M, Maximum = 50M, Value = 12M, Dock = DockStyle.Top)
        let btnSaveHall = new Button(Text = "CREATE HALL", Dock = DockStyle.Top, Height = 40, BackColor = Color.LightGreen)


        let refreshHalls () =
            cmbHalls.Items.Clear()
            match Database.getAllHalls() with
            | Ok halls -> 
                cachedHalls <- halls
                halls |> List.iter (fun h -> cmbHalls.Items.Add(sprintf "%s (%dx%d)" h.Name h.Rows h.Cols) |> ignore)
                if cmbHalls.Items.Count > 0 then cmbHalls.SelectedIndex <- 0
            | Error e -> MessageBox.Show("Failed to load halls: " + e) |> ignore


        btnSaveHall.Click.Add(fun _ ->
            Core.validateHallInputs txtHallName.Text numRows.Value numCols.Value
            |> Result.bind Database.insertHall
            |> function
               | Ok _ -> MessageBox.Show("Hall Created!"); refreshHalls(); txtHallName.Text <- ""
               | Error e -> MessageBox.Show("Error: " + e) |> ignore
        )

        btnSaveMovie.Click.Add(fun _ ->
            Core.validateMovieInputs txtTitle.Text txtTime.Text numPrice.Value cmbHalls.SelectedIndex cachedHalls
            |> Result.bind (fun movie ->
                Database.ensureNoScheduleConflict movie
                |> Result.map (fun _ -> movie)
            )
            |> Result.bind Database.insertMovie
            |> function
               | Ok _ -> MessageBox.Show("Movie Saved!"); onDataChanged(); form.Close()
               | Error e -> MessageBox.Show("Error: " + e) |> ignore
        )


        let pageMovie = new TabPage("Add Movie")
        pageMovie.Padding <- Padding(20)

        pageMovie.Controls.AddRange([| 
            btnSaveMovie; 
            cmbHalls; new Label(Text="Select Hall", Dock=DockStyle.Top, AutoSize=true); 
            numPrice; new Label(Text="Price", Dock=DockStyle.Top, AutoSize=true); 
            txtTime; new Label(Text="Time", Dock=DockStyle.Top, AutoSize=true); 
            txtTitle; new Label(Text="Title", Dock=DockStyle.Top, AutoSize=true) 
        |])
        
        let pageHall = new TabPage("Create Hall")
        pageHall.Padding <- Padding(20)
        pageHall.Controls.AddRange([| 
            btnSaveHall; 
            numCols; new Label(Text="Columns", Dock=DockStyle.Top, AutoSize=true); 
            numRows; new Label(Text="Rows", Dock=DockStyle.Top, AutoSize=true); 
            txtHallName; new Label(Text="Hall Name", Dock=DockStyle.Top, AutoSize=true) 
        |])

        tabs.Controls.Add(pageMovie)
        tabs.Controls.Add(pageHall)
        form.Controls.Add(tabs)
        
        refreshHalls()
        form

    let createCinemaForm (movie: Movie) =

        let hall = match movie.HallSnapshot with Some h -> h | None -> { Id="err"; Name="Error"; Rows=5; Cols=5 }
        
        let form = new Form(Text = sprintf "%s (%s)" movie.Title hall.Name, AutoScroll = true)

        form.Size <- Size(Math.Min(hall.Cols * 60 + 50, 800), Math.Min(hall.Rows * 60 + 100, 600))
        
        let panel = new FlowLayoutPanel(Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White)
        let btns = ResizeArray<SeatButton>()
        
        let refreshSeats() =
            match Database.getBookedSeats movie.Id with
            | Ok booked -> btns |> Seq.iter (fun b -> b.Render(booked))
            | Error _ -> ()

        let handleBooking r c =
            let ticket = Core.createTicket movie hall r c
            
            Database.bookSeat movie.Id r c ticket.Id
            |> Result.bind (fun _ -> FileSystem.saveTicket ticket)
            |> function
               | Ok _ -> MessageBox.Show("Booked! Ticket saved to disk.") |> ignore; refreshSeats()
               | Error e -> MessageBox.Show(e) |> ignore

        
        for r in 0 .. hall.Rows - 1 do
            for c in 0 .. hall.Cols - 1 do
                let btn = new SeatButton(r, c, handleBooking)
                btns.Add(btn)
                panel.Controls.Add(btn)
        
        form.Controls.Add(panel)
        
        
        let timer = new Timer(Interval = 2000)
        timer.Tick.Add(fun _ -> refreshSeats())
        timer.Start()
        form.FormClosed.Add(fun _ -> timer.Stop())
        
        refreshSeats()
        form

    let createMenuForm () =
        let form = new Form(Text = "Cinema City", Size = Size(500, 600))
        let moviePanel = new FlowLayoutPanel(Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke)
        let btnAdmin = new Button(Text = "ADMIN DASHBOARD", Dock = DockStyle.Bottom, Height = 60, BackColor = Color.LightSlateGray, ForeColor = Color.White)

        let rec loadMovies () =
            moviePanel.Controls.Clear()
            match Database.getAllMovies() with
            | Error e -> 
                let lbl = new Label(Text = "Database Error:\n" + e, AutoSize = true, ForeColor = Color.Red)
                moviePanel.Controls.Add(lbl)
            | Ok movies when movies.IsEmpty ->
                let lbl = new Label(Text = "No movies found.\nClick Admin to add one.", AutoSize = true, Font = new Font("Arial", 12.0f))
                moviePanel.Controls.Add(lbl)
            | Ok movies ->
                for m in movies do
                    let hallName = match m.HallSnapshot with Some h -> h.Name | None -> "Unknown Hall"
                    let btn = new Button(Text = sprintf "%s\n%s\n%s" m.Title m.Time hallName, Width = 220, Height = 100, BackColor = Color.White)
                    btn.Margin <- Padding(10)
                    btn.Click.Add(fun _ -> 
                        let map = createCinemaForm m
                        form.Hide()
                        map.ShowDialog() |> ignore
                        form.Show()
                    )
                    moviePanel.Controls.Add(btn)

        btnAdmin.Click.Add(fun _ -> 
            let input = Microsoft.VisualBasic.Interaction.InputBox("Enter Admin Password:", "Admin Login", "")
            if input = Config.ADMIN_PASSWORD then
                (createAdminForm loadMovies).ShowDialog() |> ignore
            else
                MessageBox.Show("Wrong Password") |> ignore
        )
        form.Controls.AddRange([| moviePanel; btnAdmin |])
        
        loadMovies()
        form