package com.javafx.demo;

import javafx.application.Application;
import javafx.scene.Scene;
import javafx.scene.control.Button;
import javafx.scene.layout.StackPane;
import javafx.stage.Stage;

public class JavaFx01 extends Application {

    public static void main(String[] args) {
        launch(args);
    }

    @Override
    public void start(Stage primaryStage) {
        // 创建一个按钮
        Button btn = new Button("点击我开始使用");

        // 布局容器
        StackPane root = new StackPane();
        root.getChildren().add(btn);

        // 创建场景
        Scene scene = new Scene(root, 400, 300);

        // 加载 CSS 样式表
        scene.getStylesheets().add(getClass().getResource("style.css").toExternalForm());

        // 设置窗口属性
        primaryStage.setTitle("JavaFX 初始界面");
        primaryStage.setScene(scene);
        primaryStage.show();
    }
}
