﻿#ExternalChecksum("..\..\KinectSensorChooser.xaml","{406ea660-64cf-4c82-b6f0-42d48172a799}","1C3B03826A5587A1ACC3ED7834E04ED4")
'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.239
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports Microsoft.Samples.Kinect.WpfViewers
Imports System
Imports System.Diagnostics
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Ink
Imports System.Windows.Input
Imports System.Windows.Markup
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Effects
Imports System.Windows.Media.Imaging
Imports System.Windows.Media.Media3D
Imports System.Windows.Media.TextFormatting
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.Windows.Shell

Namespace Microsoft.Samples.Kinect.WpfViewers
    
    '''<summary>
    '''KinectSensorChooser
    '''</summary>
    <Microsoft.VisualBasic.CompilerServices.DesignerGenerated(),  _
     System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")>  _
    Partial Public Class KinectSensorChooser
        Inherits System.Windows.Controls.UserControl
        Implements System.Windows.Markup.IComponentConnector
        
        
        #ExternalSource("..\..\KinectSensorChooser.xaml",31)
        <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
        Friend WithEvents MessageTextBlock As System.Windows.Controls.TextBlock
        
        #End ExternalSource
        
        
        #ExternalSource("..\..\KinectSensorChooser.xaml",33)
        <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
        Friend WithEvents TellMeMore As System.Windows.Controls.TextBlock
        
        #End ExternalSource
        
        
        #ExternalSource("..\..\KinectSensorChooser.xaml",34)
        <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
        Friend WithEvents TellMeMoreLink As System.Windows.Documents.Hyperlink
        
        #End ExternalSource
        
        
        #ExternalSource("..\..\KinectSensorChooser.xaml",38)
        <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
        Friend WithEvents RetryButton As System.Windows.Controls.Button
        
        #End ExternalSource
        
        Private _contentLoaded As Boolean
        
        '''<summary>
        '''InitializeComponent
        '''</summary>
        <System.Diagnostics.DebuggerNonUserCodeAttribute()>  _
        Public Sub InitializeComponent() Implements System.Windows.Markup.IComponentConnector.InitializeComponent
            If _contentLoaded Then
                Return
            End If
            _contentLoaded = true
            Dim resourceLocater As System.Uri = New System.Uri("/Microsoft.Samples.Kinect.WpfViewers;component/kinectsensorchooser.xaml", System.UriKind.Relative)
            
            #ExternalSource("..\..\KinectSensorChooser.xaml",1)
            System.Windows.Application.LoadComponent(Me, resourceLocater)
            
            #End ExternalSource
        End Sub
        
        <System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never),  _
         System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes"),  _
         System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"),  _
         System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")>  _
        Sub System_Windows_Markup_IComponentConnector_Connect(ByVal connectionId As Integer, ByVal target As Object) Implements System.Windows.Markup.IComponentConnector.Connect
            If (connectionId = 1) Then
                Me.MessageTextBlock = CType(target,System.Windows.Controls.TextBlock)
                Return
            End If
            If (connectionId = 2) Then
                Me.TellMeMore = CType(target,System.Windows.Controls.TextBlock)
                Return
            End If
            If (connectionId = 3) Then
                Me.TellMeMoreLink = CType(target,System.Windows.Documents.Hyperlink)
                
                #ExternalSource("..\..\KinectSensorChooser.xaml",34)
                AddHandler Me.TellMeMoreLink.RequestNavigate, New System.Windows.Navigation.RequestNavigateEventHandler(AddressOf Me.TellMeMoreLinkRequestNavigate)
                
                #End ExternalSource
                Return
            End If
            If (connectionId = 4) Then
                Me.RetryButton = CType(target,System.Windows.Controls.Button)
                
                #ExternalSource("..\..\KinectSensorChooser.xaml",38)
                AddHandler Me.RetryButton.Click, New System.Windows.RoutedEventHandler(AddressOf Me.RetryButtonClick)
                
                #End ExternalSource
                Return
            End If
            Me._contentLoaded = true
        End Sub
    End Class
End Namespace

