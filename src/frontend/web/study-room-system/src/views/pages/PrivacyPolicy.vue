<script setup lang="ts">
import { Card, CardHeader, CardContent } from '@/components/ui/card'
</script>

<template>
  <div class="flex justify-center p-6">
    <Card class="max-w-3xl w-full">
      <CardHeader>
        <h2 class="text-2xl font-bold">隐私政策</h2>
      </CardHeader>
      <CardContent class="space-y-4 text-sm leading-relaxed">
        <p>本系统高度重视用户的隐私与个人信息保护。请您在使用本系统前仔细阅读以下内容：</p>
        <ol class="list-decimal list-inside space-y-2">
          <li>信息收集：我们可能会收集您在注册、登录、预约时提供的基本信息（如用户名、联系方式），以及系统使用过程中产生的日志数据。</li>
          <li>信息使用：收集的信息仅用于提供和优化服务，包括身份验证、预约管理、系统安全维护等。</li>
          <li>信息共享：我们不会将您的信息出售或出租给任何第三方。仅在法律要求或为保障系统安全的情况下，才会与相关机构共享。</li>
          <li>信息存储与安全：我们采取合理的技术措施保护您的信息安全，防止未经授权的访问、泄露或篡改。</li>
          <li>用户权利：您有权查询、更正或删除您的个人信息，并可随时撤回同意。</li>
          <li>政策更新：我们可能会根据法律法规或服务需要更新隐私政策，更新后的内容将在系统中公布。</li>
        </ol>
        <p>如您继续使用本系统，即表示您已阅读并同意本隐私政策。</p>
      </CardContent>
    </Card>
  </div>
</template>
