namespace CinemaApp.Tests

open Xunit
open System.IO
open CinemaApp
open Domain

module FileSystemTests =

    [<Fact>]
    let ``saveTicket creates a file`` () =
        let ticket = {
            Id = "TEST-TICKET-1"
            Content = "TEST CONTENT"
        }

        let result = FileSystem.saveTicket ticket

        Assert.True(result.IsOk)
        Assert.True(File.Exists(ticket.Id + ".txt"))

        // Cleanup
        File.Delete(ticket.Id + ".txt")
