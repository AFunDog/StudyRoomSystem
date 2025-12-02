package com.javafx.demo;

import javafx.scene.Cursor;
import javafx.scene.Scene;
import javafx.stage.Stage;

public class ResizeHelper {
    private static final int BORDER = 6; // 鼠标距离窗口边缘多少像素时触发缩放
    private static final double MIN_WIDTH = 520; // 限制窗口最小宽度
    private static final double MIN_HEIGHT = 100; // 限制窗口最小高度

    public static void addResizeListener(Stage stage, Scene scene) {
        // 鼠标移动时，判断是否在边缘，并改变光标形状
        scene.setOnMouseMoved(e -> {
            double x = e.getSceneX();
            double y = e.getSceneY();
            double w = stage.getWidth();
            double h = stage.getHeight();

            if (x < BORDER && y < BORDER) {
                scene.setCursor(Cursor.NW_RESIZE); // 左上角
            } else if (x > w - BORDER && y < BORDER) {
                scene.setCursor(Cursor.NE_RESIZE); // 右上角
            } else if (x < BORDER && y > h - BORDER) {
                scene.setCursor(Cursor.SW_RESIZE); // 左下角
            } else if (x > w - BORDER && y > h - BORDER) {
                scene.setCursor(Cursor.SE_RESIZE); // 右下角
            } else if (x < BORDER) {
                scene.setCursor(Cursor.W_RESIZE); // 左边
            } else if (x > w - BORDER) {
                scene.setCursor(Cursor.E_RESIZE); // 右边
            } else if (y < BORDER) {
                scene.setCursor(Cursor.N_RESIZE); // 上边
            } else if (y > h - BORDER) {
                scene.setCursor(Cursor.S_RESIZE); // 下边
            } else {
                scene.setCursor(Cursor.DEFAULT); // 默认光标
            }
        });

        // 鼠标拖动时，根据光标类型调整窗口大小
        scene.setOnMouseDragged(e -> {
            double x = e.getSceneX();
            double y = e.getSceneY();
            double screenX = e.getScreenX();
            double screenY = e.getScreenY();

            if (scene.getCursor() == Cursor.E_RESIZE) {
                // 右边拖动：调整宽度
                if (x >= MIN_WIDTH) {
                    stage.setWidth(x);
                }
            } else if (scene.getCursor() == Cursor.S_RESIZE) {
                // 下边拖动：调整高度
                if (y >= MIN_HEIGHT) {
                    stage.setHeight(y);
                }
            } else if (scene.getCursor() == Cursor.SE_RESIZE) {
                // 右下角拖动：同时调整宽度和高度
                if (x >= MIN_WIDTH) {
                    stage.setWidth(x);
                }
                if (y >= MIN_HEIGHT) {
                    stage.setHeight(y);
                }
            } else if (scene.getCursor() == Cursor.W_RESIZE) {
                // 左边拖动：调整宽度，同时移动窗口 X 坐标
                double newWidth = stage.getWidth() - (screenX - stage.getX());
                if (newWidth >= MIN_WIDTH) {
                    stage.setX(screenX);
                    stage.setWidth(newWidth);
                }
            } else if (scene.getCursor() == Cursor.N_RESIZE) {
                // 上边拖动：调整高度，同时移动窗口 Y 坐标
                double newHeight = stage.getHeight() - (screenY - stage.getY());
                if (newHeight >= MIN_HEIGHT) {
                    stage.setY(screenY);
                    stage.setHeight(newHeight);
                }
            } else if (scene.getCursor() == Cursor.NW_RESIZE) {
                // 左上角拖动：同时调整宽度和高度
                double newWidth = stage.getWidth() - (screenX - stage.getX());
                double newHeight = stage.getHeight() - (screenY - stage.getY());
                if (newWidth >= MIN_WIDTH) {
                    stage.setX(screenX);
                    stage.setWidth(newWidth);
                }
                if (newHeight >= MIN_HEIGHT) {
                    stage.setY(screenY);
                    stage.setHeight(newHeight);
                }
            }
        });
    }
}
