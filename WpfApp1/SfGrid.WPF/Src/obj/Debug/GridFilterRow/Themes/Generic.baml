   M S B A M L   `   `   `  ����a  ]Syncfusion.SfGrid.WPF, Version=15.3460.0.26, Culture=neutral, PublicKeyToken=c5c2046a6f6ddf3cU/clr-namespace:Syncfusion.UI.Xaml.Grid.RowFilter!Syncfusion.UI.Xaml.Grid.RowFilter  5      A%clr-namespace:Syncfusion.UI.Xaml.GridSyncfusion.UI.Xaml.Grid  5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089a ]Syncfusion.Shared.Wpf, Version=15.3460.0.26, Culture=neutral, PublicKeyToken=3d67ed1f87d44c89   FilterOptionButtonStyle  �� 
TargetType �� Property �� Value �� 
TargetType'   !System.Windows.VisualStateManager    VisualStateGroups%  System.Windows.VisualStateGroup   States   System.Windows.VisualState   
Storyboard �� 
TargetName �� TargetProperty8  2System.Windows.Media.Animation.EasingColorKeyFrame	 �� KeyTime
 �� Value9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value ��Name F�Name �� Left
 �� Top �� Stretch  GridFilterRowCellStyle9   3Syncfusion.UI.Xaml.Grid.RowFilter.GridFilterRowCell J� SnapsToDevicePixels g� 	BeginTime g� Duration<  6System.Windows.Media.Animation.EasingThicknessKeyFrame p� KeyTime p� Value&    Syncfusion.UI.Xaml.Grid.GridCell   CellSelectionBrush    SelectionBorderVisibility � ColumnDefinitions ��Name d�Name#   FilterOptionButtonVisibility � FocusVisualStyle   CurrentCellBorderBrush!   CurrentCellBorderThickness  J� IsHitTestVisible"!   CurrentCellBorderVisibility" �Name# � AllowsTransparency$ � 	StaysOpen% � PlacementTarget& �� ElementName' ��Name$( �� IsSynchronizedWithCurrentItem) � 	Resources* �� BasedOn  FilterRowControlStyle8   2Syncfusion.UI.Xaml.Grid.RowFilter.FilterRowControl+ ��Name6	   0Syncfusion.UI.Xaml.Grid.VirtualizingCellsControl, 	  CurrentFocusBorderMargin- �� StrokeDashArray . 	  CurrentFocusRowVisibility/ 	  RowHoverBackgroundBrush"0 	  RowHighlightBorderThickness1 	  HighlightBorderClipRect2 J� Clip(3 	  !HighlightSelectionBorderVisiblity4 	  RowBackgroundClip5 	  RowSelectionBrush6 	  SelectionBorderClipRect7 	  SelectionBorderVisiblity�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      9local/clr-namespace:Syncfusion.UI.Xaml.Grid.RowFilter  5      0shared!http://schemas.syncfusion.com/wpf    5      2dataGrid%clr-namespace:Syncfusion.UI.Xaml.Grid  5      %�  &	        &	 b    7   '  )    7  &	 F    '  R    7  �� 5      "  d�6,   .�5	   
   ��  � ��
 ���� 6'   �� 5
   
    � ��
 �����6(   �� 5   
    � ��$ 0q�6,   �� 5   
    � ��
 �����6'   �� 5   
    � ��$ Center=�67   �� 5   
    � ��$ Center=�65   �� 5   
    � ��$ 0q�6$   �� 5   
    � ��$ 24��6"   �� 5   
    � �� 5      �� 5      " d�6"   .R�5      �  5        5      CommonStates��. 5   "     
Normal��6.     5   "   Pressed��. 5   &   �� .��5   *   �� $" PART_FilterOptionInnerBorder��6G   $0 *(Panel.Background).(SolidColorBrush.Color)�6|   .g�5   .     $	 0��6B   $
 	#FF119EDA��6N   5   +   5   '   5   #     5   "   Disabled��. 5    &   �� .��5!   *   U� $" PART_FilterOptionInnerBorder��6H   $ (UIElement.Opacity)�6}   .\�5"   .     $ 0��6C   $	 0.5Q�6O   5#   +   5$   '   5%   #     5&   "   	MouseOver��. 5'   &   �� .��5(   *   �� $" PART_FilterOptionInnerBorder��6G   $0 *(Panel.Background).(SolidColorBrush.Color)�6|   .g�5)   .     $	 0��6B   $
 	#FFD8D8D9��6N   5*   +   5+   '   5,   #   5-      .+�5/      �� $" PART_FilterOptionInnerBorder��6!   $��Stretch=�50   !   $��Stretch=�51   !   #��z��52   !   #��z��53   !   #��z��54   !   ._�55      � .+�56   "   F� $&  PART_FilterOptionButtonPresenter��6'   $��8.485��57   '   $��5.657��58   '   ���	#FF231F20$ 0��5:   '   $ 0��5;   '   $��Center=�5<   '   $��Center=�5=   '   "����    T� � 	= <�[ � z T� $
 Fill=�5?   '   5@      5A      5B      5C      5E      5F      �� 5I      "   5J      .�5K   
   ��  � ��
 ���� 6'   �� 5L   
    � ��
 �����6(   �� 5M   
    � ��$ 0,0,1,1q�6,   �� 5N   
    � ��$ 0,0,0,0q�6$   �� 5O   
    �@�FocusVisualStyle 6-   Z� �� 5P   
    � �� .  6&   �� 5Q   
    � ��$ Center=�65   �� 5R   
    � �� 5S      �� 5T      "  6"   .R�5U      �  .  5V        5W      BorderStates��. 5X   "     
NormalCell��6.     5Y   "   FooterColumnCell��. 5Z   &   �� $ 0X�61   .��5[   *   r� $ 0X�6K   $ 1D�5\   K   $ PART_GridCellBorder��5]   K   $ BorderThickness�5^   K   .	�5_   .     $ 0��6F   $ 1,0,1,1q�5`   F   5a   +   5b   '   5c   #     5d   "   BeforeFooterColumnCell��. 5e   &   �� $ 0X�61   .��5f   *   r� $ 0X�6K   $ 1D�5g   K   $ PART_GridCellBorder��5h   K   $ BorderThickness�5i   K   .	�5j   .     $ 0��6F   $ 0,0,0,1q�5k   F   5l   +   5m   '   5n   #   5o      .+�5q      �� #��z 6!    . #y�z 5s   !   �� 5u      $ PART_GridCellBorder��6!   #��z��5v   !   #��z��5w   !   #��z��5x   !    . ._�5{      �  5|   "   �� 5}   &   �� 5~   &   $
��Auto �67   .+�5�   "   �� $ PART_ContentPresenter��5�   3   $��0��5�   3   #��z��5�   3   d� 5�   "   $ PART_FilterOptionButton��6/   $��1��5�   /   $��Stretch=�5�   /   $��Stretch=�5�   /   #y�z 5�   /   ��.  8��   5�   /   Z� 5�   /   5�      5�      �� 5�      
������ #��z 5�   !   #��z 5�   !     .   . #y�z! 5�   !   �� 5�      $ PART_PopupPresenter��6!   ._�5�      � $" PART_FilterOptionPopup��6$   # .  $��Bottom=�5�   #   $ .  $��Slide=�5�   #   % 5�   #   �� $& PART_FilterOptionButton��.��5�   "   �� $ PART_PopUpBorder��6)   
�������$	��140��5�   -   ._�5�   &   �� $' PART_FilterOptionsList��6.   $
( TrueY�5�   .   $��Stretch=�5�   .   $��Stretch=�5�   .   $
��Left=�5�   .   $��Center=�5�   .    5�   .   Z� ) 5�   *   ���� 5�   .   '��       "  ��64   .�5�   2   ��  �@�FocusVisualStyle 6U   Z� 5�   /   5�   +   5�   '   5�   #   5�      5�      5�      5�      5�      5�      �� 5�      8*   "   6>   �� 5�      "   5�      .�5�   
   ��  � ��
 ���� 6'   �� 5�   
    � ��
 �����6(   �� 5�   
    � ��$ 0q�6,   �� 5�   
    � �� .  6&   �� 5�   
    �@�FocusVisualStyle 6-   Z� �� 5�   
    � �� 5�      �� 5�      "  6"   .R�5�      �  5�        5�      BorderStates��. 5�   "     	NormalRow��6.     5�   "   	FrozenRow��. 5�   &   �� $ 0X�61   .��5�   *   r� $ 0X�6K   $ 1D�5�   K   $ PART_RowBorder��5�   K   $ BorderThickness�5�   K   .	�5�   .     $ 0��6F   $ 
0, 0, 0, 1q�6R   5�   +   5�   '   5�   #     5�   "   	FooterRow��. 5�   &   �� $ 0X�61   .��5�   *   r� $ 0X�6K   $ 1D�5�   K   $ PART_RowBorder��5�   K   $ BorderThickness�5�   K   .	�5�   .     $ 0��6F   $ 
0, 1, 0, 0q�6R   5�   +   r� 5�   *   $ 0X�6K   $ 1D�5�   K   $ PART_RowBorder��5�   K   $ Margin�5�   K   .	�5�   .     $ 0��6F   $ 0, -1, 0, 0q�6R   5�   +   5�   '   5�   #   5�      .+�5�      �� $ PART_RowBorder��6!   #��z��5�      #��z��5�      5�      �� 5�      $+ PART_CurrentFocusRow��6$   #��z, 5�   $   
�������$	- 2,2R�5�   $   $��1��5�   $   #y�z. 5�   $   �� 5�      #��z/ 6!   #��z/ 5�   !   #��z0 5�   !   #2 z1 5�   !    . #y�z3 5�   !   �� 5�      #2 z4 6$   #��z��6O   �� 5�      #��z5 6!   #2 z6 5�   !   #y�z7 5�   !   �� 5�      #��z��6!   #��z��5�   !   ._�5�      �� 5�      5�      5�      5�      5�      �� 5      8*   "   6=   5     