package com.javafx.demo;

import javafx.application.Application;
import javafx.geometry.Insets;
import javafx.geometry.Pos;
import javafx.scene.Scene;
import javafx.scene.control.*;
import javafx.scene.layout.BorderPane;
import javafx.scene.layout.HBox;
import javafx.scene.layout.StackPane;
import javafx.scene.layout.VBox;
import javafx.scene.paint.Color;
import javafx.stage.Stage;
import javafx.stage.StageStyle;
import org.kordamp.ikonli.javafx.FontIcon;
import org.kordamp.ikonli.materialdesign2.MaterialDesignC;
import org.kordamp.ikonli.materialdesign2.MaterialDesignM;
import org.kordamp.ikonli.materialdesign2.MaterialDesignW;

public class Login extends Application {

    @Override
    public void start(Stage primaryStage) {

        // 自定义标题栏按钮（用 Ikonli 图标）
        Button minimizeBtn = new Button();
        minimizeBtn.setGraphic(new FontIcon(MaterialDesignM.MINUS_BOX)); // 最小化

        Button maximizeBtn = new Button();
        maximizeBtn.setGraphic(new FontIcon(MaterialDesignW.WINDOW_MAXIMIZE)); // 最大化矩形

        Button restoreBtn = new Button();
        restoreBtn.setGraphic(new FontIcon(MaterialDesignW.WINDOW_RESTORE)); // 还原矩形右上角带直角
        restoreBtn.setVisible(false); // 初始隐藏

        Button closeBtn = new Button();
        closeBtn.setGraphic(new FontIcon(MaterialDesignC.CLOSE)); // 关闭

        // 样式美化
        minimizeBtn.getStyleClass().add("title-bar-button");
        maximizeBtn.getStyleClass().add("title-bar-button");
        restoreBtn.getStyleClass().add("title-bar-button");
        closeBtn.getStyleClass().add("title-bar-button");

        // 功能绑定
        minimizeBtn.setOnAction(e -> primaryStage.setIconified(true));

        maximizeBtn.setOnAction(e -> {
            primaryStage.setMaximized(true);
            maximizeBtn.setVisible(false);
            restoreBtn.setVisible(true);
        });

        restoreBtn.setOnAction(e -> {
            primaryStage.setMaximized(false);
            maximizeBtn.setVisible(true);
            restoreBtn.setVisible(false);
        });

        closeBtn.setOnAction(e -> primaryStage.close());

        // 标题栏容器
        HBox titleBar = new HBox(10, minimizeBtn, maximizeBtn, restoreBtn, closeBtn);
        titleBar.setAlignment(Pos.TOP_RIGHT);
        titleBar.setPadding(new Insets(5));



        // 一级标题：用户登录
        Label mainTitle = new Label("用户登录");
        mainTitle.setStyle("-fx-font-size: 26px; -fx-font-weight: bold;");

        // 二级标题：欢迎语
        Label subTitle = new Label("欢迎来到智慧自习室预约管理系统");
        subTitle.setStyle("-fx-font-size: 16px; -fx-text-fill: gray;");

        // 将两个标题放在垂直布局中
        VBox titleBox = new VBox(5);
        titleBox.setAlignment(Pos.TOP_LEFT);
        titleBox.getChildren().addAll(mainTitle, subTitle);

        FontIcon userIcon = new FontIcon("fas-user");   // FontAwesome 用户图标
        FontIcon lockIcon = new FontIcon("fas-lock");   // FontAwesome 锁图标

        // 用户名输入框 + 小头像图标
//        Label userIcon = new Label("\uD83D\uDC64"); // Unicode小头像 👤
//        userIcon.setStyle("-fx-font-size: 16px; -fx-text-fill: gray;");

        TextField userField = new TextField();
        userField.setPromptText("请输入用户名");
        userField.setPrefWidth(430); // 设置长度

        HBox userBox = new HBox(8,userIcon,userField);
        userBox.setAlignment(Pos.CENTER_LEFT);

        // 密码输入框 + 小锁图标
//        Label lockIcon = new Label("\uD83D\uDD12"); // Unicode小锁 🔒
//        lockIcon.setStyle("-fx-font-size: 16px; -fx-text-fill: gray;");

        PasswordField passwordField = new PasswordField();
        passwordField.setPromptText("请输入密码");
        passwordField.setPrefWidth(430); // 设置长度

        HBox passwordBox = new HBox(8,lockIcon,passwordField);
        passwordBox.setAlignment(Pos.CENTER_LEFT);



        // 验证码输入框
        TextField captchaField = new TextField();
        captchaField.setPromptText("请输入验证码");

        // 登录按钮
        Button loginButton = new Button("登录");
        loginButton.setMaxWidth(Double.MAX_VALUE);



        // 自动登录复选框 + 信息标志提示
        CheckBox autoLogin = new CheckBox("下次自动登录");
        Label infoIcon = new Label("i"); // 用字母 i
        infoIcon.getStyleClass().add("info-icon"); // 使用 CSS 类
        infoIcon.setPickOnBounds(true); // 扩大鼠标悬停区域

        Tooltip tooltip = new Tooltip("勾选后，登录状态保持7天；如不勾选则关闭浏览器即为退出");
        // 设置提示文字更大
        tooltip.setStyle("-fx-font-size: 14px;");
        tooltip.setShowDelay(javafx.util.Duration.millis(200)); // 0.2秒延迟
        tooltip.setHideDelay(javafx.util.Duration.millis(200));
        tooltip.setShowDuration(javafx.util.Duration.seconds(5)); // 显示5秒
        Tooltip.install(infoIcon, tooltip);

        // 放在复选框后面
        HBox autoLoginBox = new HBox(5);
        autoLoginBox.setAlignment(Pos.CENTER);
        autoLoginBox.getChildren().addAll(autoLogin, infoIcon);

        // 三个链接：忘记密码、新用户注册、手机号登录
        Hyperlink forgotPassword = new Hyperlink("忘记密码");
        Hyperlink registerLink = new Hyperlink("新用户注册");
        Hyperlink codeLoginLink = new Hyperlink("手机号登录");

        // 链接横排 + 分隔符
        HBox linkBox = new HBox(10);
        linkBox.setAlignment(Pos.CENTER);
        linkBox.getChildren().addAll(
                new Label("|"), forgotPassword,
                new Label("|"), registerLink,
                new Label("|"), codeLoginLink
        );



        // 隐私协议复选框（带链接）
        Hyperlink privacyLink = new Hyperlink("《隐私政策》");
        Hyperlink userAgreementLink = new Hyperlink("《用户协议》");
        HBox policyBox = new HBox(5);
        CheckBox agreePolicy = new CheckBox("我已阅读并同意");
        policyBox.setAlignment(Pos.CENTER_LEFT);
        policyBox.getChildren().addAll(agreePolicy, privacyLink, new Label("和"), userAgreementLink);



        // 主布局（小屏幕面板）
        VBox formBox = new VBox(15);
        formBox.setPadding(new Insets(30));
        formBox.setAlignment(Pos.TOP_CENTER);
        formBox.setMaxWidth(500);
        formBox.setMaxHeight(450);
        formBox.getChildren().addAll(
                titleBox,
                userBox,        // 用户名输入框带图标
                passwordBox,    // 密码输入框带图标
                captchaField,
                loginButton,
                autoLoginBox,
                linkBox,
                policyBox
        );

        // 给小屏幕加阴影效果
        formBox.getStyleClass().add("form-box"); // 添加 CSS 美化

        // 给表单加滚动条
        ScrollPane scrollPane = new ScrollPane(formBox);
        scrollPane.setFitToWidth(true);   // 宽度自适应
        scrollPane.setFitToHeight(true);  // 高度自适应
        scrollPane.setStyle("-fx-background-color: transparent;"); // 背景透明

        // 外层容器：标题栏 + 表单(带滚动条)
        BorderPane rootBox = new BorderPane();
        rootBox.setTop(titleBar);       // 标题栏固定在最上方
        rootBox.setCenter(scrollPane);  // 表单部分可滚动
        rootBox.setCenter(scrollPane);  // 表单固定屏幕中间

        // 最外层背景容器（灰色填满全屏，居中显示 formBox）
        StackPane root = new StackPane(rootBox);
        root.getStyleClass().add("outer-root"); // 给背景容器加样式

        // 场景设置：窗口大小固定，打开默认中等大小，不是全屏
        Scene scene = new Scene(root, 400, 450);
        primaryStage.initStyle(javafx.stage.StageStyle.TRANSPARENT); // 去掉系统标题栏，背景设置为透明
        scene.setFill(Color.TRANSPARENT); // 场景透明
        primaryStage.initStyle(StageStyle.TRANSPARENT); // 窗口透明
        primaryStage.setWidth(1200);      // 默认宽度
        primaryStage.setHeight(900);     // 默认高度
        primaryStage.centerOnScreen();   // 居中显示
        primaryStage.setTitle("智慧自习室登录");
        primaryStage.setScene(scene);

        scene.getStylesheets().add(getClass().getResource("style.css").toExternalForm());

        // 拖动窗口逻辑（放在 start 方法里，titleBar 创建完之后）
        final double[] offset = new double[2];
        titleBar.setOnMousePressed(e -> {
            offset[0] = e.getSceneX();
            offset[1] = e.getSceneY();
        });
        titleBar.setOnMouseDragged(e -> {
            primaryStage.setX(e.getScreenX() - offset[0]);
            primaryStage.setY(e.getScreenY() - offset[1]);
        });

        // 添加拖动和缩放逻辑
        ResizeHelper.addResizeListener(primaryStage, scene);
        primaryStage.show();
    }

    public static void main(String[] args) {
        launch(args);
    }
}
