   M S B A M L   `   `   `  ����a  ]Syncfusion.SfGrid.WPF, Version=15.3460.0.26, Culture=neutral, PublicKeyToken=c5c2046a6f6ddf3cA%clr-namespace:Syncfusion.UI.Xaml.GridSyncfusion.UI.Xaml.Grid  5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   FocusedThemeBrush  �� Color  TextBoxStyle �� 
TargetType �� Property �� Value  DropDownToggleButtonStyle J� RenderTransformOrigin �� Stretch  
PopupStyle % !SfMultiColumnDropDownControlStyle:    4Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl �� 
TargetType ��Name'  !System.Windows.VisualStateManager   VisualStateGroups%  System.Windows.VisualStateGroup	   States   System.Windows.VisualState
   
Storyboard g� 	BeginTime �� 
TargetName �� TargetProperty R� KeyTime R� Value �Name J� SnapsToDevicePixels � ColumnDefinitions �Name    PopupMinWidth    PopupMinHeight    PopupMaxWidth    PopupMaxHeight �� Path �� Mode �� RelativeSource    PopupBorderBrush    PopupBorderThickness    PopupBackground � RowDefinitions ��Name(   "Syncfusion.UI.Xaml.Grid.SfDataGrid   Name(   "Syncfusion.UI.Xaml.Grid.SfGridBase!   AllowDraggingColumns"   AllowEditing#   AllowFiltering$   AllowGrouping%   AllowResizingColumns &   AllowRowHoverHighlighting'    AutoGenerateColumns(   AutoGenerateColumns")    PopupDropDownGridBackground*    GridColumnSizer+   ColumnSizer,    ItemsSource-   ItemsSource.   NavigationMode/ � FocusVisualStyle0   SelectedIndex1    ResizingThumbVisibility2 n�Name3 � Cursor4 ��Name5    ReadOnly6 �� 
IsReadOnly7    TextAlignment8 �� TextAlignment9 d�Name: �� BasedOn�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      /local%clr-namespace:Syncfusion.UI.Xaml.Grid  5      %�  &	        &	      &	 �     &	     &	 �    7   7  7  7  '   ~    7  �� 5      $
  Gray��60   �� 5      " ��6!   .�5   
   ��  � ��$ Center=�65   �� 5	   
    �@��TextWrapping$ NoWrap=�6)   �� 5
   
    � ��$ 0q�6,   �� 5   
    �@��IsReadOnlyCaretVisible . 63   5      �� 5      " d�6.   .�5   
   ��  �@��	ClickMode$ Release=�6&   �� 5   
    � �� �	#FF0000006'   �� 5   
    � �� .  6&   �� 5   
    � |� .  6&   �� 5   
    � �� 5      �� 5      .R�5      �� 
�������._�5      � .+�5      F� $��12��6#   $��12��5   #   $��4q�5   #   L���p0â ����6�Ѡ�[ 6G Q@V ����6�� ��6*��Z Ѡ�[ $�o �8� �Z #��z��5   #   $ 0.5,0.5&�5   #   $ Uniform=�5   #   5      5       5!      5#      5$      �� 5&      " �6   .�5'   
   ��  �@�AllowsTransparency .  6/   �� 5(   
    � ��$ Absolute=�6&   �� 5)   
    �@�	StaysOpen .  6&   �� 5*   
    � ��$ Slide=�6+   5+      �� 5-      "   66   .�5.   
   ��  �@�FocusVisualStyle 6-   Z� �� 5/   
    � ��$ 2q�6,   �� 50   
    � ��
 �����6'   �� 51   
    � ��
 �����6(   �� 52   
    �@  PopupBorderBrush
 �����6-   �� 53   
    �@  PopupBorderThickness$ 1q�61   �� 54   
    � ��$ Stretch=�67   �� 55   
    � �� .  6&   �� 56   
   # �@  PopupDropDownGridBackground
 �����68   �� 57   
    �@  PopupBackground
 �����6,   �� 58   
    � ��
 �   �6'   �� 59   
    �@J�SnapsToDevicePixels . 60   �� 5:   
    � �� 5;      �� 5<      "   6"   .R�5=      �� $ PART_RootBorder��6   #��z��5>      #��z��5?      #��z��5@      #��z��5A      #��z��5B       5C        5D      FocusStates��.	 5E   "     Focused��.
 5F   &   �� $ 0X�61   .��5G   *   T� $ PART_RootBorder��6H   $ BorderBrush�6p   ..�5H   .   i� $ 0��6E   8   6Q   5I   +   5J   '   5K   #     5L   "   	Unfocused��6.   5M      ._�5O      � $ PART_RootGrid��6    .  5P      �� 5Q   "   $��* �63   �� 5R   "   $
��Auto �63   .+�5T      � $ 
PART_Popup��6$   #��z 5U   $   #��z 5V   $   #��z 5W   $   #��z 5X   $   8�� ��5Y   $   �� $ IsDropDownOpen�$ TwoWay=� �� *TemplatedParent+.��5]   "   �� $��PART_PopupBorder��6)   #��z 5^   )   #��z 5_   )   ._�5`   &   � #��z 6+    5a   *   �� 5b   .   $��* �6<   �� 5c   .   $
��Auto �6<   .+�5e   *   �� $ PART_PopupContent��69   .��5f   .     $  PART_SfDataGrid��6?   $��0q�5g   ?   ! .  " .  # .  $ .  % .  & . #( z' 5n   ?   #��z) 5o   ?   #+ z* 5p   ?   |�.  #- z, 5s   ?   $	. Row=�5t   ?   / 5q   ?   Z� 0 5u   ?   �� $ SelectedIndex� �� *TemplatedParent+$ TwoWay=�5x   +   � 5y   *   $��1��6/   #y�z1 6<   .+�5z   .   n� $2 PART_ThumbGripper��64   $��Right=�5{   4   $3 SizeNWSE��5|   4   ��5}   2   �� 5~   6   .R�5   :   � 
������ .+�5�   >   F� $��12��6C   $��12��5�   C   ������[+h�%�H��h�%�H��� ���[+� ���h�%� n�h�%� n�� ���� ��    h�%ѿ�� h�%ѿ�� � ��    � ���[+���H�����H��  ���[+  ������ n���� n�  ���  ���[+    �H��    �H����� ��[+��� 	���	#FF727272$
 Fill=�5�   C   5�   ;   5�   7   5�   /   5�   +   5�   '   5�   #   5�      �� 5�      $4 PART_TextBox��6&   #��z��5�   &   $��0��5�   &   #��z��5�   &   #��z��5�   &   #��z��5�   &   #��z��5�   &   #��z��5�   &   #��z��5�   &   #��z��5�   &   #��z��5�   &   #6 z5 5�   &   8�� #��z��5�   &   #8 z7 5�   &   ��5�   &   �� $
 Text� �� *TemplatedParent+$ TwoWay=�5�   &   �� 5�      $��1��6%   $��0q�5�   %   #��z��5�   %   ._�5�   "   d� $9 PART_ToggleButton��6/   $��Center=�5�   /   8�� 5�   /   5�      5�      5�      5�      5�      5�      �� 5�      8:   "   6I   5�      