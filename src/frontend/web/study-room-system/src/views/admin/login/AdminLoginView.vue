<script setup lang="ts">
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from '@/components/ui/form'
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';

import { toTypedSchema } from '@vee-validate/zod';
import z from 'zod';
import type { GenericObject } from 'vee-validate';
import { http } from '@/lib/utils';
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import type { User } from '@/lib/types/user';
import { LockKeyhole, Eye, EyeOff, UserStar } from 'lucide-vue-next';
import { restartHubConnection } from '@/lib/api/hubConnection';
import { authRequest } from '@/lib/api/authRequest';

const router = useRouter();
const schema = z.object({
  userName: z.string({ required_error: '请输入用户名' })
    .min(4, "用户名至少 4 位")
    .max(20, "用户名不能超过 20 位")
    .regex(/^[a-zA-Z0-9._]+$/, "用户名只能包含字母、数字、点或下划线"),
  password: z.string({ required_error: '请输入密码' })
    .min(8, "密码至少 8 位")
    .max(32, "密码不能超过 32 位")
    .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$/, "密码必须包含大小写字母、数字和特殊字符"),
});
const formSchema = toTypedSchema(schema)
const loginMessage = ref('');

// const isShowPassword = ref(false);
const isAdminLoginLoading = ref(false); // 管理员登录按钮 loading 状态

async function onSubmit(values: GenericObject) {
  try {
    console.log(values);
    isAdminLoginLoading.value = true; // 开始 loading
    // var res = await authRequest.login({ username: values.userName, password: values.password });

    // const token = res.token;
    // const user = res.user as User;

    //统一由后端返回错误信息
    // if (!user) {
    //   loginMessage.value = '登录失败，用户名或密码错误';
    //   return;
    // }

    restartHubConnection();
    router.push('/admin');

  }
  catch (error) {
    loginMessage.value = '登录失败，请检查用户名或密码';
    console.log(error);
  }
  finally {
    isAdminLoginLoading.value = false; // 结束 loading
  }
}

</script>
<template>
  <div class="flex flex-col items-center justify-center h-screen">
    <div class="w-5/6 max-w-xl">
      <Card>
        <CardHeader>
          <div class="flex flex-row items-center">
            <UserStar class="size-8 mr-1"></UserStar>
            <div class="text-2xl tracking-widest">管理员登录</div>
          </div>
          <div class="text-sm text-muted-foreground">欢迎来到智慧自习室预约管理系统</div>
        </CardHeader>
        <CardContent>
          <Form class="flex flex-col gap-y-4" :validation-schema="formSchema" @submit="onSubmit">
            <FormField v-slot="{ componentField }" name="userName">
              <FormItem>
                <FormLabel>用户名</FormLabel>
                <FormControl>
                  <Input v-bind="componentField" placeholder="请输入用户名" />
                </FormControl>
                <!-- <FormDescription>用户注册时的名称</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <FormField v-slot="{ componentField }" name="password">
              <FormItem>
                <FormLabel>密码</FormLabel>
                <FormControl>
                  <div class="flex flex-row items-center relative">
                    <div class="absolute flex flex-row justify-center items-center w-10">
                      <LockKeyhole class="size-4"></LockKeyhole>
                    </div>
                    <Input v-bind="componentField" type="password" autocomplete="current-password" placeholder="请输入密码"
                      class="pl-10">
                    </Input>

                    <!-- <div class="absolute flex flex-row justify-center items-center w-10 right-0">
                      <component :is="isShowPassword ? EyeOff : Eye" @click="isShowPassword = !isShowPassword"
                        class="size-4"></component>
                    </div> -->
                  </div>
                </FormControl>
                <!-- <FormDescription>登录密码</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <div>{{ loginMessage }}</div>
            <div class="flex flex-row justify-center items-center gap-x-4">
              <Button class="hover:cursor-pointer" type="submit" :disabled="isAdminLoginLoading">
                <Loader2 v-if="isAdminLoginLoading" class="size-4 animate-spin mr-2" />
                登录
              </Button>
              <!-- <Button class="hover:cursor-pointer" type="button" variant="secondary"
                @click="router.push('/register')">注册</Button> -->
            </div>
          </Form>
        </CardContent>
      </Card>
    </div>
  </div>
</template>