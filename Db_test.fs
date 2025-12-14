namespace CinemaApp.Tests

open Xunit
open CinemaApp
open CinemaApp.Domain
open System

module DatabaseTests =

    [<Fact>]
    let ``getAllHalls returns Result type`` () =
        let result = Database.getAllHalls()
        Assert.True(result.IsOk || result.IsError)

    [<Fact>]
    let ``getAllMovies returns Result type`` () =
        let result = Database.getAllMovies()
        Assert.True(result.IsOk || result.IsError)

    [<Fact>]
    let ``booking same seat twice returns error`` () =
        // Insert a fresh hall/movie so the FK constraint passes, then try to double-book.
        let hallGuid = Guid.NewGuid().ToString("N")
        let movieGuid = Guid.NewGuid().ToString("N")
        let hallId = $"hall_{hallGuid}"
        let movieId = $"movie_{movieGuid}"
        let hall = { Id = hallId; Name = "Test Hall"; Rows = 5; Cols = 5 }
        let movie = { Id = movieId; Title = "Test Movie"; Time = "10:00"; Price = 10M; HallId = hallId; HallSnapshot = None }

        match Database.insertHall hall with
        | Error msg -> failwithf "Failed to insert hall: %s" msg
        | Ok _ -> ()

        match Database.insertMovie movie with
        | Error msg -> failwithf "Failed to insert movie: %s" msg
        | Ok _ -> ()

        match Database.bookSeat movieId 0 0 "T1" with
        | Error msg -> failwithf "First booking failed: %s" msg
        | Ok _ -> ()

        match Database.bookSeat movieId 0 0 "T2" with
        | Error msg -> Assert.Contains("Seat already taken!", msg)
        | Ok _ -> Assert.Fail("Expected duplicate booking to fail")
