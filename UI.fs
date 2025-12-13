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

        // Important: Add controls in correct Dock order
        // 1. Add Bottom control first (so it reserves space) OR use explicit Z-ordering. 
        // In this specific array method: Index 0 is Top Z-order.
        // We add Panel (Fill) and Button (Bottom). 
        form.Controls.AddRange([| moviePanel; btnAdmin |])
        
        loadMovies()
        form