Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Web

Namespace Ventrian.PropertyAgent

    Public Class TemplateZipUtils

        ' Uses sachatraqwaen SharpZipLib DNN Wrapper method
        ' https://gist.github.com/sachatrauwaen/6900943e507ec1a3c556121d79bbd04f


        Private ZipInputStreamType As Type
        Private ZipOutputStreamType As Type

        Public Sub New()
            ZipInputStreamType = Type.[GetType]("ICSharpCode.SharpZipLib.Zip.ZipInputStream, ICSharpCode.SharpZipLib")

            If ZipInputStreamType Is Nothing Then
                ZipInputStreamType = Type.[GetType]("ICSharpCode.SharpZipLib.Zip.ZipInputStream, SharpZipLib")
            End If

            ZipOutputStreamType = Type.[GetType]("ICSharpCode.SharpZipLib.Zip.ZipInputStream, ICSharpCode.SharpZipLib")

            If ZipOutputStreamType Is Nothing Then
                ZipOutputStreamType = Type.[GetType]("ICSharpCode.SharpZipLib.Zip.ZipOutputStream, SharpZipLib")
            End If
        End Sub

        Private Function ExtractFileName(ByVal path As String) As String

            Dim extractPos As Integer = path.LastIndexOf("\") + 1
            Return path.Substring(extractPos, path.Length - extractPos).Replace("/", "_").Replace("..", ".")

        End Function

        Public Sub ZipTemplateFiles(ByVal CompressionLevel As Integer, ByVal strmZipFile As FileStream, ByVal files As String(), ByVal _template As String, ByVal _moduleID As Integer, ByVal _portalSettings As PortalSettings)
            Dim strmZipStream As Object = Nothing

            Try
                strmZipStream = ZipOutputStreamType.InvokeMember("", BindingFlags.CreateInstance, Nothing, Nothing, New Object() {strmZipFile}, Nothing)
                ZipOutputStreamType.InvokeMember("SetLevel", BindingFlags.InvokeMethod, Nothing, strmZipStream, New Object() {CompressionLevel})

                For Each file As String In files
                    Dim fullPath As String = file
                    Dim fileName As String = ExtractFileName(file).TrimStart(Convert.ToChar("\"))

                    Dim folder As String = ""
                    If (fileName.ToLower() <> "template.xml") Then
                        folder = fullPath.Replace(_portalSettings.HomeDirectoryMapPath & "PropertyAgent\" & _moduleID.ToString() & "\Templates\" & _template, "").Replace(fileName, "").TrimStart(Convert.ToChar("\"))
                    End If

                    If ((_portalSettings.HomeDirectoryMapPath & "PropertyAgent\" & _moduleID.ToString() & "\Templates\" & _template & "\template.xml").ToLower() <> fullPath.ToLower()) Then
                        GetType(FileSystemUtils).InvokeMember("AddToZip", BindingFlags.InvokeMethod, Nothing, Nothing, New Object() {strmZipStream, fullPath, fileName, ""})
                    End If
                Next

            Finally

                If strmZipStream IsNot Nothing Then
                    ZipOutputStreamType.InvokeMember("Finish", BindingFlags.InvokeMethod, Nothing, strmZipStream, Nothing)
                    ZipOutputStreamType.InvokeMember("Close", BindingFlags.InvokeMethod, Nothing, strmZipStream, Nothing)
                End If
            End Try
        End Sub

        Public Sub UnzipTemplateFiles(ByVal stream As Stream, ByVal PhysicalPath As String)
            Dim strmZipStream As Object = Nothing
            strmZipStream = ZipInputStreamType.InvokeMember("", BindingFlags.CreateInstance, Nothing, Nothing, New Object() {stream}, Nothing)
            GetType(FileSystemUtils).InvokeMember("UnzipResources", BindingFlags.InvokeMethod, Nothing, Nothing, New Object() {strmZipStream, PhysicalPath})
        End Sub

    End Class
End Namespace






