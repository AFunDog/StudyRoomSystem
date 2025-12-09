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
import { adminLoginSchema } from "@/lib/validation/loginSchema";
import { useForm, } from 'vee-validate';
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import type { User } from '@/lib/types/User';
import { LockKeyhole, UserStar, Loader2 } from 'lucide-vue-next';
import { restartHubConnection } from '@/lib/api/hubConnection';
import { authRequest } from '@/lib/api/authRequest';
import { toast } from 'vue-sonner';

const router = useRouter();

//引入登录校验规则
const formSchema = toTypedSchema(adminLoginSchema);
const form = useForm({ validationSchema: formSchema });

const loginMessage = ref('');

// const isShowPassword = ref(false);

const isAdminLoginLoading = ref(false); // 管理员登录按钮 loading 状态

const onSubmit = form.handleSubmit(async (values) => {
  try {
    console.log(values);
    isAdminLoginLoading.value = true; // 开始 loading
    const res = await authRequest.login({ username: values.userName, password: values.password });

    // const token = res.token;
    const user = res.user as User;
        if (!user || user.role !== 'Admin') {
      loginMessage.value = '登录失败，您不是管理员';
      toast.error('权限不足，只有管理员可以登录');
      return;
    }

    //统一由后端返回错误信息
    // if (!user) {
    //   loginMessage.value = '登录失败，用户名或密码错误';
    //   return;
    // }

    restartHubConnection();
    toast.success('欢迎回来，管理员！');
    router.push('/admin');

  }
  catch (error) {
    loginMessage.value = '登录失败，请检查用户名或密码';
    console.log(error);
  }
  finally {
    isAdminLoginLoading.value = false; // 结束 loading
  }
});
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
          <form class="flex flex-col gap-y-4" @submit="onSubmit">
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
          </form>
        </CardContent>
      </Card>
    </div>
  </div>
</template>