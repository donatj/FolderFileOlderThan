Imports System.IO

Public Class Form1

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        txtPath.Text = My.Settings.lastPath
        Label1.Text = ""
        Label2.Text = ""
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        BackgroundWorker1.WorkerReportsProgress = True
        BackgroundWorker1.RunWorkerAsync(DateCombo.Text)
        Button1.Enabled = False
        BtnBrowse.Enabled = False
    End Sub


    Private Sub BtnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnBrowse.Click
        Dim i As New FolderBrowserDialog

        i.SelectedPath = txtPath.Text
        i.ShowDialog()
        txtPath.Text = i.SelectedPath
        My.Settings.lastPath = i.SelectedPath
        My.Settings.Save()
    End Sub


    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim myDateBefore As Date = Date.Now
        myDateBefore = myDateBefore.AddMonths(0 - e.Argument)
        Dim theState As New myState

        Dim folders As String() = IO.Directory.GetDirectories(txtPath.Text)
        theState.max = folders.Count

        Dim filename As String = My.Computer.FileSystem.SpecialDirectories.Desktop.ToString & "\FileDateOutput" & Date.Now.Ticks & ".csv"
        Dim oWrite As New StreamWriter(filename)
        oWrite.AutoFlush = True
        oWrite.WriteLine("Path,Create,Modify,Access")

        For Each folder As String In folders

            Dim newestC As New Date(0)
            Dim newestM As New Date(0)
            Dim newestA As New Date(0)

            theState.cur += 1
            theState.cur2 = 0

            theState.name = "Scanning: " & folder
            BackgroundWorker1.ReportProgress(1, theState)

            Dim files As String() = filescan(folder).ToArray

            theState.name = folder
            BackgroundWorker1.ReportProgress(1, theState)
            theState.max2 = files.Count

            Dim j As Integer = 0
            For Each file As String In files

                If True Then

                    Dim fileCdate = IO.File.GetCreationTime(file)
                    If newestC < fileCdate Then
                        newestC = fileCdate
                    End If

                    Dim fileMdate = IO.File.GetLastWriteTime(file)
                    If newestM < fileMdate Then
                        newestM = fileMdate
                    End If

                    Dim fileAdate = IO.File.GetLastAccessTime(file)
                    If newestA < fileAdate Then
                        newestA = fileAdate
                    End If

                End If

                j += 1
                theState.cur2 = j
                theState.total += 1
                If (j Mod 10 = 0) Then
                    BackgroundWorker1.ReportProgress(1, theState)
                End If
            Next

            If newestM < myDateBefore Or e.Argument < 1 Then
                oWrite.WriteLine("""" & folder & """" & "," & newestC & "," & newestM & "," & newestA)
            End If

        Next

        oWrite.Close()

        MsgBox("Done!")
        BackgroundWorker1.ReportProgress(1, theState)

    End Sub


    Class myState
        Public name As String
        Public max As Integer
        Public cur As Integer
        Public max2 As Integer
        Public cur2 As Integer
        Public total As Integer
    End Class


    Private Sub BackgroundWorker1_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        Label1.Text = e.UserState.name
        Label2.Text = "Total Files Scanned: " & e.UserState.total
        ProgressBar1.Maximum = e.UserState.max
        ProgressBar1.Value = e.UserState.cur
        ProgressBar2.Maximum = e.UserState.max2
        ProgressBar2.Value = e.UserState.cur2
    End Sub


    Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        Button1.Enabled = True
        BtnBrowse.Enabled = True
    End Sub


    Private Sub DateCombo_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles DateCombo.TextChanged

        Try
            DateCombo.Text = Convert.ToInt16(DateCombo.Text)
        Catch ex As Exception
            DateCombo.Text = 0
        End Try

    End Sub

    Function dirscan(ByVal path As String) As List(Of DirectoryInfo)
        On Error Resume Next
        Dim i As New DirectoryInfo(path)
        Dim paths As New List(Of DirectoryInfo)
        For Each directory As DirectoryInfo In i.GetDirectories()

            If directory.Name.Chars(0) <> "." And directory.Name <> "html2pdf" And directory.Name <> "test" And directory.Name <> "cache" And directory.Name <> "ckeditor" And directory.Name <> "modules" Then
                paths.Add(directory)
                Dim j As List(Of DirectoryInfo) = dirscan(directory.FullName)
                paths.AddRange(j)
            End If

        Next

        Return paths
    End Function

    Function filescan(ByVal path As String) As List(Of String)
        On Error Resume Next
        Dim dirs As List(Of DirectoryInfo) = dirscan(path)
        Dim paths As New List(Of String)

        Dim l As New DirectoryInfo(path)
        For Each j In l.GetFiles()
            paths.Add(j.FullName)
        Next

        For Each dirr As DirectoryInfo In dirs

            For Each j In dirr.GetFiles()
                paths.Add(j.FullName)
            Next

        Next

        Return paths
    End Function

End Class
