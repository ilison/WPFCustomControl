   M S B A M L   `   `   `  ����a  ]Syncfusion.SfGrid.WPF, Version=15.3460.0.26, Culture=neutral, PublicKeyToken=c5c2046a6f6ddf3cA%clr-namespace:Syncfusion.UI.Xaml.GridSyncfusion.UI.Xaml.Grid  5      ]3clr-namespace:Syncfusion.UI.Xaml.Controls.DataPager%Syncfusion.UI.Xaml.Controls.DataPager  5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   HorizontalScrollBarStyle  �� 
TargetType �� Property �� Value �� 
TargetType �Name � 	Resources  Key'   !System.Windows.VisualStateManager    VisualStateGroups%  System.Windows.VisualStateGroup   States   System.Windows.VisualState   
Storyboard	 g� Duration
 �� 
TargetName �� TargetProperty	 W� To ��Name J� Opacity R� KeyTime R� Value  System.Windows.Visibility J� IsHitTestVisible � ColumnDefinitions ��Name �� Interval  RepeatButtonTemplate n�Name  HorizontalThumbTemplate  PagerScrollViewerStyle5   /Syncfusion.UI.Xaml.Grid.ScrollableContentViewer �� CornerRadius � RowDefinitions8   2Syncfusion.UI.Xaml.Grid.ScrollableContentPresenter  Name   Owner   TemplatedParent ��Name   ScrollableHeight � Maximum � Minimum �� Orientation    ViewportHeight! �� ViewportSize*"   #ComputedVerticalScrollBarVisibility#   VerticalOffset$ � Value%   ScrollableWidth&   ViewportWidth,'   %ComputedHorizontalScrollBarVisibility(   HorizontalOffset  NavigationButtonStyle9  3System.Windows.Media.Animation.EasingDoubleKeyFrame) O� KeyTime* O� Value+ A�Name, ��Name- ��Name. �� StrokeDashArray/ �� StrokeDashOffset0 �� StrokeEndLineCap  SfDataPagerStyle7   1Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager<  6System.Windows.Media.Animation.EasingThicknessKeyFrame1 p� KeyTime2 p� Value8	  2System.Windows.Media.Animation.EasingInt32KeyFrame3 �� KeyTime4 �� Value5 �� KeyTime6 �� Value7  Name$8   HorizontalScrollBarVisibility"9   VerticalScrollBarVisibility>
   8Syncfusion.UI.Xaml.Controls.DataPager.NumericButtonPanel: 
 Name; ��Name<   AccentBackground=   AccentForeground> �� Stretch? �� CenterX@ �� CenterYA �� BasedOn  NumericButtonStyle9   3Syncfusion.UI.Xaml.Controls.DataPager.NumericButtonB g� 	BeginTimeC   HighlightingThemeBrushD �� RadiusXE �� RadiusY�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      .grid%clr-namespace:Syncfusion.UI.Xaml.Grid  5      =local3clr-namespace:Syncfusion.UI.Xaml.Controls.DataPager  5      %3B  &	        7  7  7  &	 f    7   &	 x    &	 �    7  7  7  7  7  '  =    7  &	 2=    '  �A    7  �� 5      "  ��6-   .�5   
   ��  � ��$ 17��6%   �� 5   
    � ��$ 17��6&   �� 5	   
    � ��
 ���� 6'   �� 5
   
    � ��
 ���� 6'   �� 5   
    � ��
 ���� 6(   �� 5   
    � �� .  6&   �� 5   
    � �� 5      �� 5      " ��6"   .R�5      � $
 Root��6    5      ���� 5      RepeatButtonTemplate " ��6K   .R�5   "   � $
 Root��6'   $��4��5   '   
������� 5   &     5   *   CommonStates��. 5   .     
Normal��6:   5   +   5   #   5      �� 5      HorizontalThumbTemplate " n�6N   .R�5   "   � $��6��6'    5   &     5    *   CommonStates��. 5!   .     
Normal��6:     5"   .   PointerOver��. 5#   2   �� .��5$   6   W� $	 0:0:0D�6F   $
 BackgroundPointerOver��5%   F   $ Opacity�5&   F   $ 1X�5'   F   W� 5(   6   $	 0:0:0D�6F   $
 
Background��5)   F   $ Opacity�5*   F   $ 0X�5+   F   5,   3   5-   /     5.   .   Pressed��. 5/   2   �� .��50   6   W� $	 0:0:0D�6F   $
 BackgroundPressed��51   F   $ Opacity�52   F   $ 1X�53   F   W� 54   6   $	 0:0:0D�6F   $
 
Background��55   F   $ Opacity�56   F   $ 0X�57   F   58   3   59   /     5:   .   Disabled��. 5;   2   �� .��5<   6   W� $	 0:0:0D�6F   $
 ThumbVisual��5=   F   $ Opacity�5>   F   $ 0X�5?   F   5@   3   5A   /   5B   +   .+�5D   &   � $ ThumbVisual��6+   $��0q�5E   +   
�����w�.+�5G   *   �� $ 
Background��61   $��1q�6E   �� 5H   *   $ BackgroundPointerOver��61   $��1q�5I   1   $ 0Q�5J   1   �� 5K   *   $ BackgroundPressed��61   $��1q�5L   1   $ 0Q�5M   1   5N   '   5O   #   5P      5Q       5R        5S      CommonStates��. 5T   "     
Normal��6.     5U   "   PointerOver��6.     5V   "   Disabled��. 5W   &   �� .��5X   *   W� $	 0D�6:   $

 Root��5Y   :   $ Opacity�5Z   :   $	 0.5X�5[   :   5\   '   5]   #   5^        5_      ScrollingIndicatorStates��. 5`   "     TouchIndicator��. 5a   &   �� .��5b   *   T� $	 0D�6H   $
 HorizontalRoot��5c   H   $ 
Visibility�5d   H   ..�5e   .   i� $ 0��6E    5f   2    5g   6   	Collapsed=�6L   5i   /   5j   +   5k   '   5l   #     5m   "   MouseIndicator��. 5n   &   �� .��5o   *   T� $	 0D�6H   $
 HorizontalPanningRoot��5p   H   $ 
Visibility�5q   H   ..�5r   .   i� $ 0��6E    5s   2    5t   6   	Collapsed=�6L   5v   /   5w   +   T� 5x   *   $
 HorizontalRoot��6H   $ IsHitTestVisible�6o   ..�5y   .   i� $ 0��6E   $
 True��6Q   5z   +   5{   '   5|   #   5}      .+�5      � $ HorizontalRoot��6    .   5�      �� 5�   "   $
��Auto �63   �� 5�   "   $
��Auto �63   �� 5�   "   $
��Auto �63   �� 5�   "   $��* �63   �� 5�   "   $
��Auto �63   .+�5�      �� $��5��6(   $��2��5�   (   $��0q�5�   (   �� 5�      $ HorizontalLargeDecrease��6+   $��1��5�   +   $��0��5�   +   $��2��5�   +   $��Stretch=�5�   +   $��Stretch=�5�   +   $ 50��5�   +   ��.  8��  5�   +   n� 5�      $ HorizontalThumb��6$   $��2��5�   $   $��10��5�   $   $��48��5�   $   #��z��5�   $   8�� 5�   $   �� 5�      $ HorizontalLargeIncrease��6+   $��3��5�   +   $��2��5�   +   $��Stretch=�5�   +   $��Stretch=�5�   +   $ 50��5�   +   ��.  8�� 5�   +   5�      � 5�      $ HorizontalPanningRoot��6   $��66��5�      $
��Left=�5�      .+�5�      �� $ HorizontalPanningThumb��6%   $��4��5�   %   $��17��5�   %   5�      5�      5�      5�      5�      �� 5�      "   6+   .�5�   
   ��  � �� 5�      �� 5�      "  6"   .R�5�      �� #��z��6   #��z��5�      $ 2��5�      ._�5�      � #��z��6    5�      �� 5�   "   $��* �63   �� 5�   "   $
��Auto �63    5�      �� 5�   "   $��* �60   �� 5�   "   $
��Auto �60   .+�5�        $! PART_ScrollContentPresenter��6>   $��2��5�   >   $��2��5�   >   #��z��5�   >   $��Center=�5�   >   #��z��5�   >   # z 5�   >   �� 5�      $ PART_VerticalScrollBar��6(   $��1��5�   (   $��Right=�5�   (   ��.  # z 5�   (   $ 0Q�5�   (   $ Vertical=�5�   (   #! z  5�   (   #y�z" 5�   (   #$ z# 5�   (   �� 5�      $ PART_HorizontalScrollBar��6(   $��1��5�   (   $��Bottom=�5�   (   ��.  # z% 5�   (   $ 0Q�5�   (   $ 
Horizontal=�5�   (   8��  #! z& 5�   (   #y�z' 5�   (   #$ z( 5�   (   �� 5�      $ PART_ScrollBarSeparator��6%   $��1��5�   %   $��1��5�   %   
�������
�������5�   %   5�      5�      5�      5�      5�      �� 5�      "  ��6*   .�5�   
   ��  � ��
 ���� 6'   �� 5�   
    � ��
 �   �6'   �� 5�   
    � ��
 ���� 6(   �� 5�   
    � ��$ 0q�6,   �� 5�   
    � ��$
 Left=�60   �� 5�   
    � ��$ Center=�6.   �� 5�   
    � ��$ SemiBold"�6'   �� 5�   
    �@�FocusVisualStyle 6-   Z� �� 5�   
    � ��$ 12(�6%   �� 5�   
    � �� 5�      �� 5�      " ��6"   .R�5�      �  5�        5�      CommonStates��. 5�   "     
Normal��. 5�   &   �� .��5�   *   W� $	 0D�6:   $
 PART_PageIndicator��5�   :   $ Opacity�5�   :   $	 0.8X�5�   :   5�   '   5�   #     5�   "   	MouseOver��. 5   &   �� .��5  *   W� $	 0D�6:   $
 PART_PageIndicator��5  :   $ Opacity�5  :   $ 1X�5  :   5  '   5  #     5  "   Pressed��. 5  &   �� .��5	  *   W� $	 0D�6:   $
 PART_PageIndicator��5
  :   $ Opacity�5  :   $ 1X�5  :   5  '   5  #     5  "   Disabled��. 5  &   �� .��5  *   U� $
 PART_PageIndicator��6H   $ (UIElement.Opacity)�6s   .\�5  .     $) 0��6C   $* 1Q�6O     5  .   $) 0:0:0.1��6C   $	* 0.4Q�6U   5  +   U� 5  *   $
 PART_ContentPresenter��6H   $ (UIElement.Opacity)�6v   .\�5  .     $) 0��6C   $* 1Q�6O     5  .   $) 0:0:0.1��6C   $	* 0.4Q�6U   5  +   5  '   5  #   5       5     FocusStates��. 5  "     Focused��. 5  &   �� .��5  *   W� $	 0D�6:   $
 PART_FocusVisualWhite��5   :   $ Opacity�5!  :   $ 1X�5"  :   W� 5#  *   $	 0D�6:   $
 PART_FocusVisualBlack��5$  :   $ Opacity�5%  :   $ 1X�5&  :   5'  '   5(  #     5)  "   	Unfocused��6.     5*  "   PointerFocused��6.   5+     .+�5-     A� $+ PART_PageIndicator��6"   #��z��5.  "   #��z��5/  "   $��0��50  "   �� 52     $, PART_ContentPresenter��6+   #��z��53  +   #��z��54  +   #��z��55  +   #��z��56  +   #��z��57  +   �� 58     $- PART_FocusVisualWhite��6$    .  $ 0Q�5:  $   $	. 1,1R�5;  $   $	/ 1.5Q�5<  $   $0 Square=�5=  $   �� 5>     $- PART_FocusVisualBlack��6$    .  $ 0Q�5@  $   $	. 1,1R�5A  $   $	/ 0.5Q�5B  $   $0 Square=�5C  $   5D     5E     5G     5H     �� 5J     "   6%   .�5K  
   ��  � ��
 ���� 6'   �� 5L  
    � �� 5M     �� 5N     "  6"   .R�5O     �� #��z��6   #��z��5P     #��z��5Q      5R       5S     OrientationStates��. 5T  "     Vertical��. 5U  &   �� .��5V  *   r� $
 PART_ScrollViewer��6K   $ (FrameworkElement.Margin)�6u   .	�5W  .     $1 0��6F   $2 0,0,0,0q�6R   5X  +   �� 5Y  *   $
 PART_FirstPageButton��6G   $ (Grid.Column)�6t   .C�5Z  .   	  $3 0��6B   $4 2��6N   5[  +   �� 5\  *   $
 PART_FirstPageButton��6G   $ 
(Grid.Row)�6t   .C�5]  .   	  $3 0��6B   $4 0��6N   5^  +   U� 5_  *   $
 PART_FirstPageButton��6H   $V P(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)�6u   .\�5`  .     $) 0��6C   $* 90Q�6O   5a  +   �� 5b  *   $
 PART_PreviousPageButton��6G   $ (Grid.Column)�6w   .C�5c  .   	  $3 0��6B   $4 2��6N   5d  +   �� 5e  *   $
 PART_PreviousPageButton��6G   $ 
(Grid.Row)�6w   .C�5f  .   	  $3 0��6B   $4 1��6N   5g  +   U� 5h  *   $
 PART_PreviousPageButton��6H   $V P(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)�6x   .\�5i  .     $) 0��6C   $* 90Q�6O   5j  +   �� 5k  *   $
 PART_NumericPanelBorder��6G   $ (Grid.Column)�6w   .C�5l  .   	  $3 0��6B   $4 2��6N   5m  +   �� 5n  *   $
 PART_NumericPanelBorder��6G   $ 
(Grid.Row)�6w   .C�5o  .   	  $3 0��6B   $4 2��6N   5p  +   �� 5q  *   $
 PART_NextPageButton��6G   $ (Grid.Column)�6s   .C�5r  .   	  $3 0��6B   $4 2��6N   5s  +   �� 5t  *   $
 PART_NextPageButton��6G   $ 
(Grid.Row)�6s   .C�5u  .   	  $3 0��6B   $4 3��6N   5v  +   U� 5w  *   $
 PART_NextPageButton��6H   $V P(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)�6t   .\�5x  .     $) 0��6C   $* 90Q�6O   5y  +   �� 5z  *   $
 PART_LastPageButton��6G   $ (Grid.Column)�6s   .C�5{  .   	  $3 0��6B   $4 2��6N   5|  +   �� 5}  *   $
 PART_LastPageButton��6G   $ 
(Grid.Row)�6s   .C�5~  .   	  $3 0��6B   $4 4��6N   5  +   U� 5�  *   $
 PART_LastPageButton��6H   $V P(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)�6t   .\�5�  .     $) 0��6C   $* 90Q�6O   5�  +   5�  '   5�  #     5�  "   
Horizontal��. 5�  &   �� .��5�  *   �� $
 PART_FirstPageButton��6G   $ 
(Grid.Row)�6t   .C�5�  .   	  $3 0��6B   $4 2��6N   5�  +   �� 5�  *   $
 PART_FirstPageButton��6G   $ (Grid.Column)�6t   .C�5�  .   	  $3 0��6B   $4 0��6N   5�  +   �� 5�  *   $
 PART_PreviousPageButton��6G   $ 
(Grid.Row)�6w   .C�5�  .   	  $3 0��6B   $4 2��6N   5�  +   �� 5�  *   $
 PART_PreviousPageButton��6G   $ (Grid.Column)�6w   .C�5�  .   	  $3 0��6B   $4 1��6N   5�  +   �� 5�  *   $
 PART_NumericPanelBorder��6G   $ 
(Grid.Row)�6w   .C�5�  .   	  $3 0��6B   $4 2��6N   5�  +   �� 5�  *   $
 PART_NumericPanelBorder��6G   $ (Grid.Column)�6w   .C�5�  .   	  $3 0��6B   $4 2��6N   5�  +   �� 5�  *   $
 PART_NextPageButton��6G   $ 
(Grid.Row)�6s   .C�5�  .   	  $3 0��6B   $4 2��6N   5�  +   �� 5�  *   $
 PART_NextPageButton��6G   $ (Grid.Column)�6s   .C�5�  .   	  $3 0��6B   $4 3��6N   5�  +   �� 5�  *   $
 PART_LastPageButton��6G   $ 
(Grid.Row)�6s   .C�5�  .   	  $3 0��6B   $4 2��6N   5�  +   �� 5�  *   $
 PART_LastPageButton��6G   $ (Grid.Column)�6s   .C�5�  .   	  $3 0��6B   $4 4��6N   5�  +   5�  '   5�  #   5�       5�     ButtonStates��. 5�  "     
Normal��. 5�  &   �� .��5�  *   U� $
 PART_FirstPageButton��6H   $ (UIElement.Opacity)�6u   .\�5�  .     $) 0��6C   $* 1Q�6O   5�  +   U� 5�  *   $
 PART_PreviousPageButton��6H   $ (UIElement.Opacity)�6x   .\�5�  .     $) 0��6C   $* 1Q�6O   5�  +   U� 5�  *   $
 PART_NextPageButton��6H   $ (UIElement.Opacity)�6t   .\�5�  .     $) 0��6C   $* 1Q�6O   5�  +   U� 5�  *   $
 PART_LastPageButton��6H   $ (UIElement.Opacity)�6t   .\�5�  .     $) 0��6C   $* 1Q�6O   5�  +   5�  '   5�  #     5�  "   LeftButtonsDisabled��. 5�  &   �� .��5�  *   �� $
 PART_FirstPageButton��6I   $ (UIElement.IsEnabled)�6v   .p�5�  .   s� $5 0��6F   6 . 6R   s� 5�  .   $5 0:0:0.1��6F   6 .  6X   5�  +   �� 5�  *   $
 PART_PreviousPageButton��6I   $ (UIElement.IsEnabled)�6y   .p�5�  .   s� $5 0��6F   6 . 6R   s� 5�  .   $5 0:0:0.1��6F   6 .  6X   5�  +   5�  '   5�  #     5�  "   RightButtonsDisabled��. 5�  &   �� .��5�  *   �� $
 PART_NextPageButton��6I   $ (Control.IsEnabled)�6u   .p�5�  .   s� $5 0��6F   6 .  6R   5�  +   �� 5�  *   $
 PART_LastPageButton��6I   $ (Control.IsEnabled)�6u   .p�5�  .   s� $5 0��6F   6 .  6R   5�  +   5�  '   5�  #     5�  "   LeftRightButtonsDisabled��. 5�  &   �� .��5�  *   �� $
 PART_NextPageButton��6I   $ (Control.IsEnabled)�6u   .p�5�  .   s� $5 0��6F   6 .  6R   5�  +   �� 5�  *   $
 PART_LastPageButton��6I   $ (Control.IsEnabled)�6u   .p�5�  .   s� $5 0��6F   6 .  6R   5�  +   �� 5�  *   $
 PART_FirstPageButton��6I   $ (UIElement.IsEnabled)�6v   .p�5�  .   s� $5 0:0:0.1��6F   6 .  6X   5�  +   �� 5�  *   $
 PART_PreviousPageButton��6I   $ (UIElement.IsEnabled)�6y   .p�5�  .   s� $5 0:0:0.1��6F   6 .  6X   5�  +   5�  '   5�  #   5�     ._�5�     � #��z��6   $��Center=�6b    5�     �� 5�  "   $
��Auto �63   �� 5�  "   $
��Auto �63   �� 5�  "   $
��Auto �63   �� 5�  "   $
��Auto �63   �� 5�  "   $
��Auto �63    5�     �� 5�  "   $
��Auto �60   �� 5�  "   $
��Auto �60   �� 5�  "   $
��Auto �60   �� 5�  "   $
��Auto �60   �� 5�  "   $
��Auto �60   .+�5�     �� $ PART_NumericPanelBorder��6%   $��2��5�  %   $��2��5�  %   $��-2,0,0,0q�5�  %   
������ ._�5�  "     $7 PART_ScrollViewer��6?   $��Center=�5�  ?   $8 Hidden=�5�  ?   8��  $9 Hidden=�5�  ?   $y�	Collapsed=�5�  ?   .��5�  &   
  $: PART_NumericButtonPanel��6?   5   #   5     �� 5     $; PART_FirstPageButton��6%   $��2��5  %   $��0��5  %   $��25��5  %   $��25��5  %   $��5q�5  %   #��z< 5  %   8�� $y�	Collapsed=�5
  %   ��5  "   F� 5  &   $��8��6+   $��8��5  +   $��0,0,0,0q�5  +   ~���p�#�TȮ��?Q�%�TȮ��?��L;�QE4�}Wp@QR�9�}Wp@����j��        ���    ����j�����qH�    �qH	#��z= 5  +   $> Uniform=�5  +   z�5  "   V� 5  &   .~�5  *   �� $
? 12.5Q�6:   $
@ 12.5Q�6I   5  '   5     �� 5     $; PART_PreviousPageButton��6%   $��2��5  %   $��1��5  %   $��25��5  %   $��25��5  %   $��5q�5  %   #��z< 5  %   8�� $y�	Collapsed=�5!  %   ��5"  "   F� 5#  &   $��8��6+   $��8��5$  +   $��0,0,0,0q�5%  +   ����� ��&��� � �&d_� ��&X� ���'|��8O)|�Ѹu�'� ��0'LR/ ���-'&�, ��3�'^9��� )|bF��Ȉ�'|bF�� ��&(������%\�- � ��&��� #��z= 5'  +   $> Uniform=�5(  +   z�5*  "   V� 5+  &   .~�5,  *   �� $
? 12.5Q�6:   $
@ 12.5Q�6I   5-  '   5/     �� 52     $; PART_NextPageButton��6%   $��2��53  %   $��3��54  %   $��25��55  %   $��25��56  %   $��5q�57  %   #��z< 58  %   8�� $y�	Collapsed=�5:  %   ��5;  "   F� 5<  &   $��8��6+   $��8��5=  +   �����0��!����k�!N����(��!d7���ԓ |bF��Zd;�k�@|bF��x� �H����9!�. �8�<!��0 �8[� T� ���Mbn�@|��P� |��0��!�,� �@�"LW/ �0��!���#��z= 5?  +   $
> Fill=�5@  +   z�5B  "   V� 5C  &   .~�5D  *   �� $
? 12.5Q�6:   $
@ 12.5Q�6I   5E  '   5G     �� 5I     $; PART_LastPageButton��6%   $��2��5J  %   $��4��5K  %   $��25��5L  %   $��25��5M  %   $��5q�5N  %   #��z< 5O  %   8�� $y�	Collapsed=�5Q  %   ��5R  "   F� 5S  &   $��8��6+   $��8��5T  +   $��0,0,0,0q�5U  +   �����R�    ��D4    Q�D4�}Wp@QR��}Wp@QR�:@0G�`@� *�+;�5�5  �?�+;��6��4�Q    ������>Q�F�������>QR�j�q��`@	#��z= 5W  +   $> UniformToFill=�5X  +   z�5Z  "   V� 5[  &   .~�5\  *   �� $
? 12.5Q�6:   $
@ 12.5Q�6I   5]  '   5_     5`     5a     5b     5d     5e     �� 5f     8A   "   68   �� 5i     "   6'   .�5j  
   ��  � ��
 ���� 6'   �� 5k  
    � ��$ 13(�6%   �� 5l  
    � ��$ 40��6#   �� 5m  
    �@�FocusVisualStyle 6-   Z� �� 5n  
    � |� . 6&   �� 5o  
    � �� 5p     �� 5q     "  6"   .R�5r     �  5s       5t     CommonStates��. 5u  "     
Normal��6.     5v  "   	MouseOver��5}  #     5~  "   Pressed��5�  #     5�  "   CurrentPage��. 5�  &   �� $B 0X�61   .��5�  *   T� $
 PART_CurrentPageIndicator��6H   $ (UIElement.Visibility)�6z   ..�5�  .   i� $ 0:0:0��6E    5�  2    5�  6   Visible=�6J   5�  /   5�  +   5�  '   5�  #   5�     .+�5�     �� $- PART_CurrentPageIndicator��6$   $��25��5�  $   $��25��5�  $   $��Center=�5�  $   $��Center=�5�  $   #��zC 5�  $   $
D 12.5��5�  $   $
E 12.5��5�  $   �� 5�     $ PART_RootBorder��6!   #��z��5�  !   #��z��5�  !   #��z��5�  !   ._�5�     �� $, PART_ContentPresenter��6/   $��Center=�5�  /   $��Center=�5�  /   5�     5�     5�     5�     5�     �� 5�     8A   "   6:   5�     