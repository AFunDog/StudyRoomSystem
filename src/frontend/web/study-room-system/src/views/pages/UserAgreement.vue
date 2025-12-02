<script setup lang="ts">
import { Card, CardHeader, CardContent } from '@/components/ui/card'
</script>

<template>
  <div class="flex justify-center p-6">
    <Card class="max-w-3xl w-full">
      <CardHeader>
        <h2 class="text-2xl font-bold">用户协议</h2>
      </CardHeader>
      <CardContent class="space-y-4 text-sm leading-relaxed">
        <p>欢迎使用智慧自习室预约管理系统。请您在使用前仔细阅读以下条款：</p>
        <ol class="list-decimal list-inside space-y-2">
          <li>服务内容：本系统为用户提供自习室预约、管理与相关辅助功能。</li>
          <li>用户责任：
            <ul class="list-disc list-inside ml-4">
              <li>用户需保证所提供信息真实、准确。</li>
              <li>用户不得利用本系统进行违法或不当行为。</li>
              <li>用户需妥善保管账号和密码，避免泄露。</li>
            </ul>
          </li>
          <li>系统责任：
            <ul class="list-disc list-inside ml-4">
              <li>系统将尽力保证服务的稳定与安全，但不对因不可抗力或第三方原因造成的服务中断负责。</li>
              <li>系统有权在必要时对服务进行维护或升级。</li>
            </ul>
          </li>
          <li>知识产权：本系统的所有内容与功能归开发方所有，未经许可不得复制、传播或用于商业用途。</li>
          <li>协议变更：系统有权根据需要修改本协议，修改后的内容将在系统中公布。</li>
        </ol>
        <p>如您继续使用本系统，即表示您已阅读并同意本用户协议。</p>
      </CardContent>
    </Card>
  </div>
</template>
