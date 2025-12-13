namespace CinemaApp

open System.IO
open Domain

module FileSystem =
    let saveTicket (ticket: Ticket) =
        try
            File.WriteAllText(ticket.Id + ".txt", ticket.Content)
            Ok ()
        with ex -> Error ("File Write Error: " + ex.Message)