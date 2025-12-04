<template>
    <!-- 首页概览：展示统计数据 -->
    <div class="space-y-4">
        <h2 class="text-2xl font-bold mb-4">系统概览</h2>
        <div class="grid grid-cols-3 gap-4">
            <!-- TODO: 调用 /api/v1/booking/my 获取预约数量 -->
            <div class="bg-accent text-accent-foreground p-4 rounded-lg shadow">
                <h3 class="text-lg font-semibold">预约总数</h3>
                <p class="text-2xl font-bold">128</p>
            </div>
            <!-- TODO: 调用 /api/v1/room 获取房间信息并计算使用率 -->
            <div class="bg-accent text-accent-foreground p-4 rounded-lg shadow">
                <h3 class="text-lg font-semibold">房间使用率</h3>
                <p class="text-2xl font-bold">76%</p>
            </div>
            <!-- TODO: 调用 /api/v1/complaint 获取投诉数量 -->
            <div class="bg-accent text-accent-foreground p-4 rounded-lg shadow">
                <h3 class="text-lg font-semibold">投诉数量</h3>
                <p class="text-2xl font-bold">12</p>
            </div>
        </div>
    </div>
</template>