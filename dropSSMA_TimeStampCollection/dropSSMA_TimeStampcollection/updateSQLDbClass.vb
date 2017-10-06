Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Contensive.Addons.dropSSMA_TimeStampcollection
    '
    ' Sample Vb2005 addon
    '
    Public Class updateSQLDbClass
        Inherits AddonBaseClass
        '
        ' - update references to your installed version of cpBase
        ' - Verify project root name space is empty
        ' - Change the namespace to the collection name
        ' - Change this class name to the addon name
        ' - Create a Contensive Addon record with the namespace apCollectionName.ad
        ' - add reference to CPBase.DLL, typically installed in c:\program files\kma\contensive\
        '
        '=====================================================================================
        ' addon api
        '=====================================================================================
        '
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim returnHtml As String
            Dim cs As CPCSBaseClass = CP.CSNew()
            Dim cs2 As CPCSBaseClass = CP.CSNew()
            Dim tableName As String = "ccTables"
            Dim thistableName As String = ""
            Dim inc As Integer
            'Dim SSMA_TimeStamp As String = ""
            'Dim Sql As String = ""
            Try
                If cs.Open("tables") Then
                    Do
                        'SSMA_TimeStamp = cs.GetText("SSMA_TimeStamp")
                        thistableName = cs.GetText("name")
                        Dim sql As String = "IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS " _
                            & " WHERE TABLE_NAME = '" & thistableName & "'  And COLUMN_NAME = 'SSMA_TimeStamp') " _
                            & " BEGIN " _
                            & " ALTER TABLE " & thistableName & " drop column SSMA_TimeStamp " _
                            & " END "
                        CP.Db.ExecuteSQL(sql)
                        Call cs.GoNext()
                    Loop While cs.OK
                End If
                returnHtml = "Success"

            Catch ex As Exception
                errorReport(CP, ex, "execute")
                returnHtml = "Failed"
            End Try
            Return returnHtml
        End Function
        '
        '=====================================================================================
        ' common report for this class
        '=====================================================================================
        '
        Private Sub errorReport(ByVal cp As CPBaseClass, ByVal ex As Exception, ByVal method As String)
            Try
                cp.Site.ErrorReport(ex, "Unexpected error in sampleClass." & method)
            Catch exLost As Exception
                '
                ' stop anything thrown from cp errorReport
                '
            End Try
        End Sub
    End Class
End Namespace
