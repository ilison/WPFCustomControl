   M S B A M L   `   `   `  ����a  ]Syncfusion.SfGrid.WPF, Version=15.3460.0.26, Culture=neutral, PublicKeyToken=c5c2046a6f6ddf3cA%clr-namespace:Syncfusion.UI.Xaml.GridSyncfusion.UI.Xaml.Grid  5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   DetailsViewDataGridStyle  �� 
TargetType1    +Syncfusion.UI.Xaml.Grid.DetailsViewDataGrid �� Property �� Value-  'System.Windows.Input.KeyboardNavigation �� 
TargetType J� SnapsToDevicePixels ��Name �� BasedOn  DetailsViewRowControlStyle3   -Syncfusion.UI.Xaml.Grid.DetailsViewRowControl $  DetailsViewContentPresenterStyle9   3Syncfusion.UI.Xaml.Grid.DetailsViewContentPresenter ��Name �� Padding'  !System.Windows.VisualStateManager	   VisualStateGroups%  System.Windows.VisualStateGroup
   States   System.Windows.VisualState   
Storyboard g� 	BeginTime �� 
TargetName �� TargetProperty<  6System.Windows.Media.Animation.EasingThicknessKeyFrame p� KeyTime p� Value $  GridDetailsViewExpanderCellStyle9   3Syncfusion.UI.Xaml.Grid.GridDetailsViewExpanderCell   Transitions%	  System.Windows.VisualTransition 	  From	 	  To 	  GeneratedDuration 	  
Storyboard9
  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value 
  EasingFunction.  (System.Windows.Media.Animation.CubicEase7  1System.Windows.Media.Animation.EasingFunctionBase   
EasingMode  System.Windows.Visibility   Visible R� Value R� KeyTime   	Collapsed   ExpanderIconVisibility F�Name  �� Stretch! J� RenderTransformOrigin" �� Angle " GridDetailsViewIndentCellStyle7   1Syncfusion.UI.Xaml.Grid.GridDetailsViewIndentCell�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      /local%clr-namespace:Syncfusion.UI.Xaml.Grid  5      %�  &	        '   �    7   &	 �    '  �    7  &	 �    '      7  &	 6    '  �    7  &	 �    '  &    7  �� 5      "    6-   .�5   
   ��  � ��
 ���� 6'   �� 5   
    � ��
 �����6(   �� 5   
    � �� .  6&   �� 5	   
    �@ TabNavigation$
 None=�6=   �� 5
   
    � ��$ 1,1,0,0q�6,   �� 5   
    � �� 5      �� 5      "   6"   .R�5      �  . .+�5      �� #��z��6!   #��z��5   !   #��z��5   !    . ._�5      �� $ PART_ContentPresenter��6/   5      5      5      5      5      �� 5      8   "    6@   �� 5      "   6/   .�5   
   ��  � ��
 ���� 6'   �� 5   
    � ��
 �����6(   �� 5   
    � ��$ 0q�6,   �� 5    
    � �� .  6&   �� 5!   
    �@�FocusVisualStyle 6-   Z� �� 5"   
    � �� 5#      �� 5$      "  6"   .R�5%      �� #��z��6   #��z��5&      #��z��5'       . ._�5)      �� 5*      5+      5-      5.      �� 5/      8   "   6B   �� 52      "   65   .�53   
   ��  � ��
 �����6(   �� 54   
    � ��$ 0,0,1,1q�6,   �� 55   
    � �� .  6&   �� 56   
    �@�FocusVisualStyle 6-   Z� �� 57   
    � �� 58      �� 59      "  6"   .R�5:      �� $ PART_RowBorder��6   #��z��5;      #��z��5<      # z��5=      	 5?        5@      BorderStates��.
 5A   "     
NormalCell��. 5B   &   �� $ 0X�61   .��5C   *   r� $ PART_RowBorder��6K   $ (Border.BorderThickness)�6r   .	�5D   .     $ 0��6F   $ 0,0,1,0q�6R   5E   +   5F   '   5G   #     5I   "   LastCell��. 5J   &   �� $ 0X�61   .��5K   *   r� $ PART_RowBorder��6K   $ (Border.BorderThickness)�6r   .	�5L   .     $ 0��6F   $ 0,0,1,1q�6R   5M   +   5N   '   5O   #   5P      ._�5S      �� 5T      5U      5W      5X      �� 5Y      8   "   6H   �� 5\      "   65   .�5]   
   ��  � �� �	#FF4D4D4D6'   �� 5^   
    � ��
 �����6(   �� 5_   
    � ��$ 0,0,1,1q�6,   �� 5`   
    � ��$ 12(�6%   �� 5a   
    �@�FocusVisualStyle 6-   Z� �� 5b   
    � �� .  6&   �� 5c   
    � �� 5d      �� 5e      "  6"   .R�5f      �� 
������ #��z��5g      #��z��5h      # z��5i      	 5j        5k      ExpansionStates�� 5l   "   	  5m   &   $ Expanded��67   $ 	Collapsed��6G   $ 0D�6V   . 5n   *   �� .��5o   .   U� $ PART_ExpanderCellPath��6L   $V P(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)�6z   .\�5p   2   
  $ 0:0:0.2��6G   $ 90Q�6Y    5q   6     5r   :   $ EaseIn=�6D   5t   3   5u   /   5v   +   5w   '   	  5x   &   $ 	Collapsed��67   $ Expanded��6H   $ 0D�6V   . 5y   *   �� .��5z   .   U� $ PART_ExpanderCellPath��6L   $V P(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)�6z   .\�5{   2   
  $ 0:0:0.2��6G   $ 0Q�6Y    5|   6     5}   :   $ EaseIn=�6D   5   3   5�   /   5�   +   5�   '   .
 5�   "     Expanded��. 5�   &   �� .��5�   *   T� $ PART_CollapseCellPath��6H   $# (FrameworkElement.Visibility)�6v   ..�5�   .   i� # Z 6E   $ 0:0:0��6k   5�   +   T� 5�   *   $ PART_ExpanderCellPath��6H   $# (FrameworkElement.Visibility)�6v   ..�5�   .   i� # Z 6E   $ 0:0:0��6m   5�   +   5�   '   5�   #     5�   "   	Collapsed��. 5�   &   �� .��5�   *   T� $ PART_CollapseCellPath��6H   $# (FrameworkElement.Visibility)�6v   ..�5�   .   i� # Z 6E   $ 0:0:0��6k   5�   +   T� 5�   *   $ PART_ExpanderCellPath��6H   $# (FrameworkElement.Visibility)�6v   ..�5�   .   i� # Z 6E   $ 0:0:0��6k   5�   +   5�   '   5�   #   5�      ._�5�      � #��z��6   #y�z 6I   #��z��6   .+�5�      F� $ PART_CollapseCellPath��6.   $��2q�6#   +����        ��-    ��-��� �    ��� 	$��9��5�   #   $��1.499��5�   #   #��z��5�   #   $  Uniform=�5�   #   F� 5�      $ PART_ExpanderCellPath��6#   $��2q�5�   #   $��9��5�   #   $��1.499��5�   #   +����        ��-    ��-��� �    ��� 	#��z��5�   #   $! 0.5,0.5&�5�   #   $  Uniform=�5�   #   z�5�   "   V� 5�   &   ~�5�   .   �� $" 90Q�6>   5�   '   5�      5�      5�      5�      5�      5�      �� 5�      8   "   6H   �� 5�      "   63   .�5�   
   ��  �@�FocusVisualStyle 6-   Z� �� 5�   
    � �� .  6&   �� 5�   
    � ��
 �����6(   �� 5�   
    � �� 5�      �� 5�      "  6"   .R�5�      �� $ PART_CellBorder��6   #��z��5�      #��z��5�      	 5�        5�      BorderStates��.
 5�   "     
NormalCell��. 5�   &   �� $ 0X�61   .��5�   *   r� $ PART_CellBorder��6K   $ (Border.BorderThickness)�6s   .	�5�   .     $ 0��6F   $ 0q�6R   5�   +   5�   '   5�   #     5�   "   LastCell��. 5�   &   �� $ 0X�61   .��5�   *   r� $ PART_CellBorder��6K   $ (Border.BorderThickness)�6s   .	�5�   .     $ 0��6F   $ 0,0,0,1q�6R   5�   +   5�   '   5�   #   5�      ._�5�      �� 5�      5�      5�      5�      �� 5�      8   "   6F   5�      