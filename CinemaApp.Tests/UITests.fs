namespace CinemaApp.Tests

open Xunit
open System
open System.Windows.Forms
open CinemaApp
open Domain

module UITests =

    [<Fact>]
    let ``Menu form is created without crashing`` () =
        let form = UI.createMenuForm()
        Assert.NotNull(form)
        Assert.Equal("Cinema City", form.Text)

    [<Fact>]
    let ``Cinema form is created for a movie`` () =
        let hall = { Id="H1"; Name="Main"; Rows=5; Cols=5 }
        let movie = {
            Id = "M1"
            Title = "Test Movie"
            Time = "18:00"
            Price = 20M
            HallId = "H1"
            HallSnapshot = Some hall
        }

        let form = UI.createCinemaForm movie

        Assert.NotNull(form)
        Assert.Contains("Test Movie", form.Text)

    [<Fact>]
    let ``Admin form is created`` () =
        let form = UI.createAdminForm (fun () -> ())
        Assert.NotNull(form)
        Assert.Equal("Admin Dashboard", form.Text)
