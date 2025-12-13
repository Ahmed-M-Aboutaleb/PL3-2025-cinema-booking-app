namespace CinemaApp

open MySqlConnector
open Domain

module Database =
    
    let private connectionString = Config.CONNECTION_STRING

    let private tryExecute f =
        try 
            f() 
        with ex -> Error ex.Message

    // Helper to run query without returning data
    let private executeNonQuery query paramsList =
        tryExecute (fun () ->
            use conn = new MySqlConnection(connectionString)
            conn.Open()
            use cmd = new MySqlCommand(query, conn)
            for (name, value) in paramsList do
                cmd.Parameters.AddWithValue(name, value) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            Ok ()
        )

    let getAllHalls () =
        tryExecute (fun () ->
            use conn = new MySqlConnection(connectionString)
            conn.Open()
            use cmd = new MySqlCommand("SELECT Id, Name, RowsInt, ColsInt FROM halls", conn)
            use reader = cmd.ExecuteReader()
            
            [ while reader.Read() do
                yield {
                    Id = reader.GetString("Id")
                    Name = reader.GetString("Name")
                    Rows = reader.GetInt32("RowsInt")
                    Cols = reader.GetInt32("ColsInt")
                } ]
            |> Ok
        )

    let getAllMovies () =
        tryExecute (fun () ->
            use conn = new MySqlConnection(connectionString)
            conn.Open()
            let query = 
                """SELECT m.Id, m.Title, m.ShowTime, m.Price, m.HallId, 
                          h.Name, h.RowsInt, h.ColsInt
                   FROM movies m
                   JOIN halls h ON m.HallId = h.Id"""
            use cmd = new MySqlCommand(query, conn)
            use reader = cmd.ExecuteReader()
            
            [ while reader.Read() do
                let hall = {
                    Id = reader.GetString("HallId")
                    Name = reader.GetString("Name")
                    Rows = reader.GetInt32("RowsInt")
                    Cols = reader.GetInt32("ColsInt")
                }
                yield {
                    Id = reader.GetString("Id")
                    Title = reader.GetString("Title")
                    Time = reader.GetString("ShowTime")
                    Price = reader.GetDecimal("Price")
                    HallId = hall.Id
                    HallSnapshot = Some hall
                } ]
            |> Ok
        )

    /// Ensures the hall/time combination is free before inserting a movie.
    let ensureNoScheduleConflict (m: Movie) =
        tryExecute (fun () ->
            use conn = new MySqlConnection(connectionString)
            conn.Open()
            use cmd = new MySqlCommand("SELECT COUNT(*) FROM movies WHERE HallId = @hid AND ShowTime = @time", conn)
            cmd.Parameters.AddWithValue("@hid", m.HallId) |> ignore
            cmd.Parameters.AddWithValue("@time", m.Time) |> ignore
            let count = cmd.ExecuteScalar() :?> int64
            Ok count
        )
        |> Result.bind (fun count ->
            if count > 0L then Error "Another movie is already scheduled in this hall at the same time."
            else Ok ()
        )

    let insertMovie (m: Movie) =
        let result =
            executeNonQuery 
                "INSERT INTO movies (Id, Title, ShowTime, Price, HallId) VALUES (@id, @title, @time, @price, @hid)" 
                [ ("@id", box m.Id); ("@title", box m.Title); ("@time", box m.Time); ("@price", box m.Price); ("@hid", box m.HallId) ]

        match result with
        | Error msg when msg.Contains("Duplicate entry") && msg.Contains("hall_time") ->
            Error "Another movie is already scheduled in this hall at the same time."
        | other -> other

    let insertHall (h: Hall) =
        executeNonQuery 
            "INSERT INTO halls (Id, Name, RowsInt, ColsInt) VALUES (@id, @name, @r, @c)" 
            [ ("@id", box h.Id); ("@name", box h.Name); ("@r", box h.Rows); ("@c", box h.Cols) ]

    let getBookedSeats movieId =
        tryExecute (fun () ->
            use conn = new MySqlConnection(connectionString)
            conn.Open()
            use cmd = new MySqlCommand("SELECT RowIdx, ColIdx FROM bookings WHERE MovieId = @mid", conn)
            cmd.Parameters.AddWithValue("@mid", movieId) |> ignore
            use reader = cmd.ExecuteReader()
            
            [ while reader.Read() do yield (reader.GetInt32(0), reader.GetInt32(1)) ]
            |> Set.ofList
            |> Ok
        )

    let bookSeat movieId r c ticketId =
        let result = 
            executeNonQuery 
                "INSERT INTO bookings (MovieId, RowIdx, ColIdx, CustomerTicketId, BookingTime) VALUES (@mid, @r, @c, @tid, NOW())"
                [ ("@mid", box movieId); ("@r", box r); ("@c", box c); ("@tid", box ticketId) ]
        
        match result with
        | Error msg when msg.Contains("Duplicate entry") -> Error "Seat already taken!"
        | other -> other