   M S B A M L   `   `   `  ����K  GAuctionServerWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:AuctionServerWpfAuctionServerWpf  5   	   !    AuctionServerWpf.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:AuctionServerWpf     5   	   Hxclb9http://schemas.microsoft.com/netfx/2009/xaml/presentation    5   	   -   5          � Title$  
MainWindow��5	   	   $��410.2��6P   $	��868��6_   .��5
      � .+�5   
   �� -   5         ��Name$ 
StartTrade��5      $��Start Trade��6>   $
��Left=�6T   $��30��6o   $��	57,57,0,0q�6{   $	��Top=�6�   $	��110��6�   �� 5   
   -   5         ��Name$ 
TradeTimer��5      $
��Left=�6(   $
��Wrap=�6C   $��Timer��6W   $	��Top=�6d   $��
296,57,0,0q�6|   $��26(�6�   $��30��6�   $	��110��6�   �� 5   
   -   5        $ 	ErrorText��5      
���  ��$
��Left=�68   $
��Wrap=�6S   $��	ErrorText��6g   $	��Top=�6x   $��
57,265,0,0q�6�   $��14(�6�   $��30��6�   $	��401��6�   �� 5   
   -   5        $ LastRequestMethodName��5      $
��Left=�63   $
��Wrap=�6N   $��LastRequest��6b   $	��Top=�6u   $��
57,230,0,0q�6�   $��26(�6�   $��30��6�   $	��401��6�   �� 5   
   -   5        $ ProductName��5      $
��Left=�6)   $
��Wrap=�6D   $��Product name��6X   $	��Top=�6l   $��189,109,0,0q�6�   $��26(�6�   $��40��6�   $	��301��6�   �� 5   
   -   5        $ 	ProductId��5      $
��Left=�6'   $
��Wrap=�6B   $��ID��6V   $	��Top=�6`   $��
57,109,0,0q�6x   $��26(�6�   $��30��6�   $	��110��6�   �� 5   
   -   5        $ 	StartTime��5      $
��Left=�6'   $
��Wrap=�6B   $��	StartTime��6V   $	��Top=�6g   $��
57,160,0,0q�6   $��26(�6�   $��30��6�   $	��110��6�   �� 5   
   -	   5        $��	TradeList��5      $	��369��6M   $��490,10,10,0q�6Z   $	��Top=�6o   ��6*   �� *
TradesList+��5      �� 5      .R�5      � $	��310��6%    � 
ItemHeight$	 150��61    � 	ItemWidth$	 150��6B    � Orientation$ 
Horizontal=�6R   5      ��5      �� 5      .R�5      �� $	��150��6!   $	��150��6-   .+�5      �� $	��110��6    $	��150��6-   ��69   �� *ImageSource+�� 5      $��20��6$   $��14(�60   $��Bottom=�6>   $��Center=�6Y   ��6v   �� *ProductName+6�   �� 5      $��20��6$   $��14(�60   $��Bottom=�6>   $��Center=�6Y   ��6v   �� *MaxBet+6�   5      5      5!      5#      5$      