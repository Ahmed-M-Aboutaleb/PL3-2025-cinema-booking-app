namespace CinemaApp

open System
open Domain

module Core =

    let validateHallInputs (name: string) (rows: decimal) (cols: decimal) =
        if String.IsNullOrWhiteSpace(name) then Error "Hall name cannot be empty."
        else if rows < 2M || cols < 2M then Error "Hall must be at least 2x2."
        else 
            Ok { 
                Id = Guid.NewGuid().ToString().Substring(0,8)
                Name = name
                Rows = int rows
                Cols = int cols 
            }

    let validateMovieInputs (title: string) (time: string) (price: decimal) (hallIdx: int) (halls: Hall list) =
        if String.IsNullOrWhiteSpace(title) then Error "Title is required."
        else if String.IsNullOrWhiteSpace(time) then Error "Time is required."
        else if hallIdx < 0 || hallIdx >= halls.Length then Error "Please select a valid hall."
        else
            let hall = halls.[hallIdx]
            Ok { 
                Id = Guid.NewGuid().ToString().Substring(0,8)
                Title = title
                Time = time
                Price = price
                HallId = hall.Id
                HallSnapshot = None
            }
    
    let createTicket (movie: Movie) (hall: Hall) r c =
        let id = sprintf "TKT-%s-%d%d-%s" movie.Id (r+1) (c+1) (Guid.NewGuid().ToString().Substring(0,4))
        let content = sprintf "-----------------\nCINEMA TICKET\nID: %s\nMovie: %s\nSeat: %d-%d\nHall: %s\n-----------------" 
                        id movie.Title (r+1) (c+1) hall.Name
        { Id = id; Content = content }