namespace CinemaApp

module Domain =
    
    type Hall = {
        Id: string
        Name: string
        Rows: int
        Cols: int
    }

    type Movie = {
        Id: string
        Title: string
        Time: string
        Price: decimal
        HallId: string 
        HallSnapshot: Hall option 
    }

    type Ticket = { 
        Id: string
        Content: string 
    }
    
    let bind f x = Result.bind f x