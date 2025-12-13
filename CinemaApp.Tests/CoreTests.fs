namespace CinemaApp.Tests

open Xunit
open CinemaApp
open Domain

module CoreTests =

    [<Fact>]
    let ``validateHallInputs returns error when name is empty`` () =
        let result = Core.validateHallInputs "" 5M 5M
        match result with
        | Error msg -> Assert.Equal("Hall name cannot be empty.", msg)
        | Ok _ -> Assert.True(false)

    [<Fact>]
    let ``validateHallInputs returns hall when inputs are valid`` () =
        let result = Core.validateHallInputs "Hall A" 10M 12M
        match result with
        | Ok hall ->
            Assert.Equal("Hall A", hall.Name)
            Assert.Equal(10, hall.Rows)
            Assert.Equal(12, hall.Cols)
        | Error _ -> Assert.True(false)

    [<Fact>]
    let ``validateMovieInputs fails when no hall selected`` () =
        let halls =
            [ { Id="1"; Name="Main"; Rows=10; Cols=10 } ]

        let result = Core.validateMovieInputs "Movie" "18:00" 20M -1 halls
        Assert.True(result.IsError)

    [<Fact>]
    let ``validateMovieInputs succeeds with correct data`` () =
        let halls =
            [ { Id="H1"; Name="Main"; Rows=10; Cols=10 } ]

        let result = Core.validateMovieInputs "Avatar" "20:00" 30M 0 halls
        match result with
        | Ok movie ->
            Assert.Equal("Avatar", movie.Title)
            Assert.Equal("H1", movie.HallId)
        | Error _ -> Assert.True(false)

    [<Fact>]
    let ``createTicket generates ticket content`` () =
        let movie = {
            Id = "M1"
            Title = "Interstellar"
            Time = "21:00"
            Price = 50M
            HallId = "H1"
            HallSnapshot = None
        }

        let hall = { Id="H1"; Name="IMAX"; Rows=10; Cols=10 }

        let ticket = Core.createTicket movie hall 1 2

        Assert.Contains("Interstellar", ticket.Content)
        Assert.Contains("Seat: 2-3", ticket.Content)
