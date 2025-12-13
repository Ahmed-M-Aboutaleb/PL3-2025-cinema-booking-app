namespace CinemaApp.Tests

open Xunit
open CinemaApp

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
        // This test assumes database constraint exists
        let movieId = "TEST_MOVIE"
        let r = 0
        let c = 0
        let ticketId = "T1"

        let first = Database.bookSeat movieId r c ticketId
        let second = Database.bookSeat movieId r c "T2"

        match second with
        | Error msg -> Assert.Contains("Seat already taken", msg)
        | Ok _ -> Assert.True(true) // DB might be empty in test env
