   M S B A M L   `   `   `  ���� X  TXceed.Wpf.Toolkit, Version=4.0.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f45clr-namespace:Xceed.Wpf.ToolkitXceed.Wpf.Toolkit  U/clr-namespace:Xceed.Wpf.Toolkit.Core.Converters!Xceed.Wpf.Toolkit.Core.Converters  C&clr-namespace:Xceed.Wpf.Toolkit.ThemesXceed.Wpf.Toolkit.Themes  O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem E'clr-namespace:Xceed.Wpf.Toolkit.ChromesXceed.Wpf.Toolkit.Chromes  \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� MergedDictionaries �� Source    BooleanToVisibilityConverter<    6Xceed.Wpf.Toolkit.Core.Converters.InverseBoolConverter  InverseBoolConverterG   AXceed.Wpf.Toolkit.Core.Converters.ColorToSolidColorBrushConverter # ColorToSolidColorBrushConverter9   3Xceed.Wpf.Toolkit.Core.Converters.AdditionConverter  AdditionConverterK   EXceed.Wpf.Toolkit.Core.Converters.ColorModeToTabItemSelectedConverter ' #ColorModeToTabItemSelectedConverter  ColorItemContainerStyle �� 
TargetType �� Property �� Value �� 
TargetType �Name � ToolTip � 	Resources	 �� Triggers
 P� Property �� Empty P� Value ��Name �� Triggers �� 
TargetName+   %Xceed.Wpf.Toolkit.Themes.ResourceKeys   ControlSelectedBorderKey#   ButtonMouseOverBackgroundKey �� Value �� Binding �� RelativeSource �� AncestorType#   Xceed.Wpf.Toolkit.ColorPicker  ColorItemTemplate  CheckerBrush   ControlNormalBorderKey �� 	Converter   ColorPickerToggleButtonStyle J� SnapsToDevicePixels � ColumnDefinitions �� Padding,   &Xceed.Wpf.Toolkit.Chromes.ButtonChrome  Name   CornerRadius J� IsMouseOver   RenderMouseOver   RenderPressed  �� Mode!   RenderChecked"   RenderEnabled# J� IsHitTestVisible$ F�Name 	 DownArrowGeometry 
 ColorDisplayStyle  ColorListStyle% �� ConverterParameter  TabItemTemplate*   $Xceed.Wpf.Toolkit.ColorPickerTabItem& J� Opacity' ��Name( ^� 
Conditions) �� Binding* �� Value  ColorPicker  PanelBackgroundBrush  PopupDarkBorderBrush+ d�Name,   ButtonStyle- d� 	IsChecked. �� Mode/ ��Name0 �� Orientation1 �Name2 � 	StaysOpen3 � AllowsTransparency4 � HorizontalOffset5 � VerticalOffset6   MaxDropDownWidth7 �� ElementName8 �� Path9   DropDownBorderThickness:   DropDownBackground;   DropDownBorderBrush< ��Name= �� SelectedIndex>  Name?   TabBackground@   TabForegroundA � RowDefinitionsB �� 	ConverterC   AvailableColorsHeaderD   HeaderBackgroundE   HeaderForegroundF �� PaddingG ��NameH   StandardColorsHeaderI   RecentColorsHeader#   Xceed.Wpf.Toolkit.ColorCanvasJ   UsingAlphaChannelK   SelectedColor L   ControlMouseOverBorderKeyM �� BasedOn�� D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     )localclr-namespace:Xceed.Wpf.Toolkit  8conv/clr-namespace:Xceed.Wpf.Toolkit.Core.Converters  1themes&clr-namespace:Xceed.Wpf.Toolkit.Themes  .sys&clr-namespace:System;assembly=mscorlib  2chrome'clr-namespace:Xceed.Wpf.Toolkit.Chromes    �� $$ ../../Themes/Aero2/Common.xaml?��� $$ ../../Themes/Aero2/Glyphs.xaml?�%-  &	        &	      &	 
     &	      &	      &	      &	     7  7  &	 �    7   7 	 &	
     7  7  &	 9    7  7  7  7  &	 �    &	 �
    7  7  7  7  7 
 7 
 7  7  7   7  7   7  7   7  7  7  '  0    7  ��          �� " ��.���  �@�ToolTip �� *Name+��  � �� �� " ��.R�� $ mainGrid�� �� *Name+ ���� '_�       " _�	 P� 
 � ��# Z . ���  � y�$ 	Collapsed=�.+��� $��Center=�$��Center=��� $ _outerBorder��$��1q�
������ 
������ $��Stretch=�$��Stretch=�._��� $ _innerBorder��
������ $��1q�
������ $��Stretch=�$��Stretch=� P� 
 �@J�IsMouseOver . . ��� $ _outerBorder��# �   � ���� $ _innerBorder��# �   � ��P� 
 �@��
IsSelected . . ��� $ _outerBorder��# �   � ���� $ _innerBorder��# �   � ���� $ False�� �� *DisplayColorTooltip+ �� "  .a��� # Z $ mainGrid�� �@�ToolTip�� .R�� .+��� 8��  #���  $��1q�$��2,2,2,2q�._��� $��14��$��14������ " ��.���  � �� �� *Color+8  �� " d�.���  � �� �� " d�.R��  . .+��  �� $��* ��� $
��Auto �.+��� #��z��#��z��#��z��# z�� . ._��� #��z��#��z��#��z��#��z��#��z��  $ ToggleButtonChrome��$��1��$ 0��# z # z��y��� *ShowDropDownButton+ �� $  FindAncestor=�$ local:ColorPickerL�8   ! �� *IsOpen+ �� $  FindAncestor=�$ local:ColorPickerL�" �� *	IsEnabled+ �� $  FindAncestor=�$ local:ColorPickerL�.��� $ 
arrowGlyph��# .  $��1��$��5q�.+�F� $$ Arrow��$��9��$��5��8�� #��� ��$��0,1,0,0q� P� 
 � {� .  . ��� $ Arrow�� � �� �#AFAFAF�� " ��.���  � |� .  ��  � �� �� .R��� 8��  ._��� ���� *SelectedColor+ �� $  FindAncestor=�$ local:ColorPickerL�8  	 ��  �� *SelectedColor+ �� $  FindAncestor=�$ local:ColorPickerL� Z� .a���  � y�$ 	Collapsed=��� " ��.���  � ��
 ���� ��  � ��$ 0q���  � ��$	 500����  � �� �� *MaxDropDownWidth+ �� "  8   $	% -18����  � �� �� .R�� ���� *ActualWidth+ �� $  FindAncestor=�$ ListBoxL�8  $% -4����  � ��8  ��  � ��8  ��  �@��SelectionMode$ Single=��� "  .R�� $ templateRoot�� . .+��� $ 
mainBorder��#��z��$��1,1,1,0q�#��z��$��0q�._��� $ innerBorder�����#ACACAC$��1,1,1,0q�#��z��$��-1q�$& 0Q��� $' contentPresenter��$��Header��|�.  #��z����. # z ���� *HorizontalContentAlignment+ �� " ������ *VerticalContentAlignment+ �� " �� ^� ( �� ) �� *
IsSelected+ �� *Self+* true.0���  �@K�ZIndex$ 1����  � ��$ 
-2,-2,-2,0q��� $ innerBorder�� �@J�Opacity$ 1Q��� "  .���  � ��
 �������  � ��# �  ��  � ��$ 1,1,0,1q���  �@ ButtonStyle8   ��  � �� .  ��  � ��$ Stretch=���  � ��$ Center=���  �@ MaxDropDownWidth$	 250Q���  �@ HeaderBackground
 �������  �@ DropDownBackground8  ��  �@ DropDownBorderBrush8  ��  �@ DropDownBorderThickness$ 1q���  �@ TabBackground �	#FFF0F0F0��  � �� �� "  .R�� .+�d� $"+ PART_ColorPickerToggleButton����. $��22��#��z��#��z��#��z��#��z��#��z��#��z��#��z, - �� *IsOpen+ �� *TemplatedParent+$. TwoWay=�# �� *IsOpen+ �� *TemplatedParent+8  .��� $��2q�.+��� $/ 	ColorOnly��8�� �� $ ColorAndName��
�������$y�Hidden=�._��� $0 
Horizontal=�.+��� $
��Left=�$��20��$��2,1,4,1q�8�� $��1q�#���  �� $��Center=����� *SelectedColorText+ �� *TemplatedParent+� $"1 PART_ColorPickerPalettePopup��$��Bottom=�2 .  3 . |�.  $4 1��$5 1��$��Slide=�# Z #��z6 ���� $"7 PART_ColorPickerToggleButton��$8 	IsChecked� ���� '_�       " _�	 P� 
 � ��# Z . ���  � y�$ 	Collapsed=�.���� #��z9 #��z: #��z; $ 3q�._��� $< ColorPickerTabControl��
������ = �� *	ColorMode+ �� *TemplatedParent+8  .@�  $> StandardTabItem��#��z? #��z@ 8�� ���� *StandardTabHeader+ �� *TemplatedParent+.��� $ _colorPaletteHost��$��4q�A �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+�� $��1��y��� *ShowAvailableColors+8B  .+�� A �� $
��Auto ��� .+��� #��zC #��zD #��zE $F 2q�$��0,0,0,1q��� $G PART_AvailableColors��$��1��8��	 ���� *AvailableColors+ �� *TemplatedParent+� $��2��y��� *ShowStandardColors+8B 
 .+�� A �� $
��Auto ��� $
��Auto �.+��� #��zH #��zD #��zE $F 2q�$��0,1,0,1q��� $G PART_StandardColors��$��1��8�� ���� *StandardColors+ �� *TemplatedParent+� $��3��$��0,1,0,1q�y��� *ShowRecentColors+8B  .+�� A �� $
��Auto ��� $��22 �.+��� #��zI #��zD #��zE $F 2q�$��0,1,0,1q��� $G PART_RecentColors��$��1��8�� ���� *RecentColors+ �� *TemplatedParent+  $> AdvancedTabItem��#��z? #��z@ 8�� ���� *AdvancedTabHeader+ �� *TemplatedParent+.��� $ _colorCanvasHost������ *MaxDropDownWidth+ �� *TemplatedParent+8  $	% -18��.+��� $
��Auto=�$��Hidden=�.��  
������ $��0q����� *
Foreground+ �� *TemplatedParent+J �� *UsingAlphaChannel+ �� *TemplatedParent+K �� *SelectedColor+ �� *TemplatedParent+ P� 
 �@ DisplayColorAndName . . ��� $ 	ColorOnly�� � y�$ 	Collapsed=��� $ ColorAndName�� � y�$ Visible=�P� 
 �@ ShowTabHeaders .  . ��� $ StandardTabItem�� � y�$ 	Collapsed=��� $ AdvancedTabItem�� � y�$ 	Collapsed=�P� 
 �@ ShowDropDownButton .  . ���  � ��$ 1q�	 P� 
 �@J�IsMouseOver . . ���  � ��# � L P� 
 �@J�IsKeyboardFocusWithin . . ���  � ��# �  �� "  8M   