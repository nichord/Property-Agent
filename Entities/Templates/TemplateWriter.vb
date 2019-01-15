Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization

Imports DotNetNuke.Common
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Services.Exceptions


Namespace Ventrian.PropertyAgent

    Public Class TemplateWriter

#Region " Private Members "

        Private _template As String
        Private _portalSettings As PortalSettings

        Private _files As New ArrayList

        Private _moduleID As Integer
        Private _title As String
        Private _folder As String
        Private _description As String

        Private _includeTypes As Boolean = False

#End Region

#Region " Constructors "

        Public Sub New(ByVal portalSettings As PortalSettings, ByVal template As String, ByVal moduleID As Integer, ByVal title As String, ByVal folder As String, ByVal description As String, ByVal includeTypes As Boolean)

            _portalSettings = portalSettings

            _template = template
            _moduleID = moduleID

            _title = title
            _folder = folder
            _description = description

            _includeTypes = includeTypes

        End Sub

#End Region

#Region " Private Methods "

        Private Sub CreateManifest()

            'Create Manifest Document
            Dim xmlManifest As New XmlDocument

            'Root Element
            Dim nodeRoot As XmlNode = xmlManifest.CreateElement("PropertyAgent")
            nodeRoot.Attributes.Append(XmlUtils.CreateAttribute(xmlManifest, "version", "1.0"))
            nodeRoot.Attributes.Append(XmlUtils.CreateAttribute(xmlManifest, "type", "Template"))

            'Template Info
            nodeRoot.AppendChild(XmlUtils.CreateElement(xmlManifest, "Name", _title))
            nodeRoot.AppendChild(XmlUtils.CreateElement(xmlManifest, "Description", _description))

            'Custom Fields Element
            Dim nodeCustomFields As XmlNode = xmlManifest.CreateElement("CustomFields")
            nodeRoot.AppendChild(nodeCustomFields)

            Dim objCustomFieldController As New CustomFieldController
            Dim objCustomFields As List(Of CustomFieldInfo) = objCustomFieldController.List(_moduleID, True)

            For Each objCustomField As CustomFieldInfo In objCustomFields
                Dim nodeCustomField As XmlNode = xmlManifest.CreateElement("CustomField")

                'Add custom field properties
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Name", objCustomField.Name))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Caption", objCustomField.Caption))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "CaptionHelp", objCustomField.CaptionHelp))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "FieldType", objCustomField.FieldType.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "FieldElementType", objCustomField.FieldElementType.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "FieldElements", objCustomField.FieldElements))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "DefaultValue", objCustomField.DefaultValue))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Length", objCustomField.Length))

                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Required", objCustomField.IsRequired.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "ValidationType", objCustomField.ValidationType.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "RegularExpression", objCustomField.RegularExpression))

                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Searchable", objCustomField.IsSearchable.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "SearchType", objCustomField.SearchType.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "FieldElementsFrom", objCustomField.FieldElementsFrom.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "FieldElementsTo", objCustomField.FieldElementsTo.ToString()))

                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Sortable", objCustomField.IsSortable.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Featured", objCustomField.IsFeatured.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Listing", objCustomField.IsInListing.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Manager", objCustomField.IsInManager.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "CaptionHidden", objCustomField.IsCaptionHidden.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "Hidden", objCustomField.IsHidden.ToString()))
                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "LockDown", objCustomField.IsLockDown.ToString()))

                nodeCustomField.AppendChild(XmlUtils.CreateElement(xmlManifest, "InheritSecurity", objCustomField.InheritSecurity.ToString()))

                nodeCustomFields.AppendChild(nodeCustomField)
            Next

            'Custom Fields Element
            Dim nodeSettings As XmlNode = xmlManifest.CreateElement("Settings")
            nodeRoot.AppendChild(nodeSettings)

            Dim objModuleController As New ModuleController
            Dim objSettings As Hashtable = objModuleController.GetModuleSettings(_moduleID)

            Dim settings As IDictionaryEnumerator = objSettings.GetEnumerator
            While settings.MoveNext
                If (settings.Key.ToString().ToLower() <> "template") Then
                    Dim nodeSetting As XmlNode = xmlManifest.CreateElement("Setting")
                    nodeSetting.AppendChild(XmlUtils.CreateElement(xmlManifest, "Name", settings.Key.ToString))
                    If (settings.Key.ToString().ToLower() <> "propertyagenttemplate") Then
                        nodeSetting.AppendChild(XmlUtils.CreateElement(xmlManifest, "Value", settings.Value.ToString))
                    Else
                        nodeSetting.AppendChild(XmlUtils.CreateElement(xmlManifest, "Value", _folder))
                    End If
                    nodeSettings.AppendChild(nodeSetting)
                End If
            End While

            ' Types
            Dim nodePropertyTypes As XmlNode = xmlManifest.CreateElement("PropertyTypes")
            nodeRoot.AppendChild(nodePropertyTypes)

            If (_includeTypes) Then
                GetSubTypes(xmlManifest, nodePropertyTypes, -1)
            End If

            xmlManifest.AppendChild(nodeRoot)

            xmlManifest.Save(Globals.HostMapPath & "Template.xml")

            _files.Add(Globals.HostMapPath & "Template.xml")

        End Sub

        Private Sub GetSubTypes(ByRef xmlManifest As XmlDocument, ByRef objParentNode As XmlNode, ByVal parentID As Integer)

            Dim objPropertyTypeController As New PropertyTypeController
            Dim objPropertyTypes As List(Of PropertyTypeInfo) = objPropertyTypeController.List(_moduleID, True, PropertyTypeSortByType.Standard, Null.NullString(), parentID)

            For Each objPropertyType As PropertyTypeInfo In objPropertyTypes
                Dim nodePropertyType As XmlNode = xmlManifest.CreateElement("PropertyType")
                nodePropertyType.AppendChild(XmlUtils.CreateElement(xmlManifest, "Name", objPropertyType.Name))
                nodePropertyType.AppendChild(XmlUtils.CreateElement(xmlManifest, "Description", objPropertyType.Description))
                nodePropertyType.AppendChild(XmlUtils.CreateElement(xmlManifest, "SortOrder", objPropertyType.SortOrder.ToString()))
                Dim nodePropertyTypes As XmlNode = xmlManifest.CreateElement("PropertyTypes")
                nodePropertyType.AppendChild(nodePropertyTypes)
                Me.GetSubTypes(xmlManifest, nodePropertyTypes, objPropertyType.PropertyTypeID)
                objParentNode.AppendChild(nodePropertyType)
            Next

        End Sub

        Private Sub BuildFileList()

            Dim folder As String = _portalSettings.HomeDirectoryMapPath & "\PropertyAgent\" & _moduleID.ToString() & "\Templates\" & _template
            ParseFolder(folder)

        End Sub

        Private Sub ParseFolder(ByVal folder As String)

            Dim objFolder As DirectoryInfo = New DirectoryInfo(folder)

            'Recursively parse the subFolders
            Dim subFolders As DirectoryInfo() = objFolder.GetDirectories()
            For Each subFolder As DirectoryInfo In subFolders
                ParseFolder(subFolder.FullName)
            Next

            'Add the Files in the Folder
            Dim files As FileInfo() = objFolder.GetFiles()
            For Each file As FileInfo In files
                Dim path As String = objFolder.FullName & "\" & file.Name
                _files.Add(path)
            Next

        End Sub

        Private Sub ZipFiles()

            BuildFileList()
            Dim filelist As String() = _files.ToArray(GetType(System.String))
            Dim CompressionLevel As Integer = 9

            Dim zipFile As String = Globals.HostMapPath & _folder & ".zip"

            Dim strmZipFile As FileStream = Nothing
            Try

                strmZipFile = File.Create(zipFile)

                Dim strmZipStream As FileStream = Nothing
                Try
                    Dim tzu As New TemplateZipUtils()
                    tzu.ZipTemplateFiles(CompressionLevel, strmZipStream, filelist, _template, _moduleID, _portalSettings)

                Catch ex As Exception
                    LogException(ex)
                Finally
                    If Not strmZipStream Is Nothing Then
                        strmZipStream.Close()
                    End If
                End Try

            Catch ex As Exception
                LogException(ex)
            Finally
                If Not strmZipFile Is Nothing Then
                    strmZipFile.Close()
                End If
            End Try

            System.IO.File.Delete(Globals.HostMapPath & "Template.xml")

        End Sub

#End Region

#Region " Public Methods "

        Public Sub CreateTemplate()

            CreateManifest()
            ZipFiles()

        End Sub

#End Region

    End Class

End Namespace
