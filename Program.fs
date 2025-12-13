namespace CinemaApp

open System
open System.Windows.Forms
open UI

module Program =
    [<EntryPoint>]
    let main argv =
        Application.SetHighDpiMode(HighDpiMode.SystemAware) |> ignore
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(false)
        
        let mainForm = createMenuForm()
        Application.Run(mainForm)
        0