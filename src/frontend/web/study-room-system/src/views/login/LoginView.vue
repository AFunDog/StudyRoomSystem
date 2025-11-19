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
import { useForm, type GenericObject } from 'vee-validate';
import { http } from '@/lib/Utils';
import { onMounted, onUnmounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import type { User } from '@/lib/types/User';
import { LockKeyhole, Eye, EyeOff, Loader2, Info } from 'lucide-vue-next';
import { restartHubConnection } from '@/lib/api/HubConnection';
import { useConfig } from '@/lib/Config';
import { toast } from 'vue-sonner';
import { AxiosError } from 'axios';
import { authRequest } from '@/lib/api/AuthRequest';
import { Checkbox } from '@/components/ui/checkbox';

const router = useRouter();
const schema = z.object({
  userName: z.string({ required_error: '请输入用户名' }).min(4, '请输入用户名'),
  password: z.string({ required_error: '请输入密码' }).min(6, '请输入密码'),
  agreePolicy: z.preprocess(val => val === 'on' || val === true, z.boolean().refine(value => value, '请同意隐私政策')),
  autoLogin: z.preprocess(val => val === 'on' || val === true, z.boolean()).optional(),
});
const formSchema = toTypedSchema(schema)
const isShowPassword = ref(false);
const isLoginLoading = ref(false);
const form = useForm({ validationSchema: formSchema });
const onSubmit = form.handleSubmit(async (values) => {
  try {
    isLoginLoading.value = true;
    var res = await authRequest.login({ username: values.userName, password: values.password });
    // var res = await http.post('/auth/login', {
    //   userName: values.userName,
    //   password: values.password
    // })

    const token = res.token;
    const user = res.user as User;

    if (!token || !user) {
      toast.error('登录失败，用户名或密码错误');
      return;
    }
    console.log("登录成功", token, user);
    toast.success('登录成功');
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
    http.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    restartHubConnection();
    router.push('/');

  }
  catch (error) {
    if (error instanceof AxiosError)
      toast.error('登录失败', { description: error.response?.data.message });
    else
      toast.error('登录失败');
    console.log(error);
  }
  finally {
    isLoginLoading.value = false;
  }
});

onMounted(() => {
  // 登录页面不显示底部标签
  const config = useConfig();
  config.isBottomTagShow = false;
});

onUnmounted(() => {
  const config = useConfig();
  config.isBottomTagShow = true;
});

</script>
<template>
  <div class="flex flex-col items-center justify-center h-full">
    <div class="w-5/6 max-w-120">
      <Card>
        <CardHeader>
          <div class="text-2xl tracking-widest">登录</div>
          <div class="text-sm text-muted-foreground">欢迎来到智慧自习室预约管理系统</div>
        </CardHeader>
        <CardContent class="flex flex-col gap-y-4">
          <form class="flex flex-col gap-y-4" :validation-schema="formSchema" @submit="onSubmit">
            <FormField v-slot="{ componentField }" name="userName">
              <FormItem>
                <FormLabel>用户名</FormLabel>
                <FormControl>
                  <Input v-bind="componentField" autocomplete="username" placeholder="请输入用户名" />
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
            <FormField v-slot="{ componentField }" name="agreePolicy">
              <FormItem>
                <FormControl>
                  <div>
                    <div class="flex flex-row gap-x-2 items-center">
                      <Checkbox v-bind="componentField">
                      </Checkbox>
                      <div class="text-sm [&>a]:text-primary">
                        我已阅读并同意
                        <a href="">《隐私政策》</a>
                        和
                        <a href="">《用户协议》</a>
                      </div>
                    </div>
                  </div>
                </FormControl>
                <!-- <FormDescription>登录密码</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <FormField v-slot="{ componentField }" name="autoLogin">
              <FormItem>
                <FormControl>
                  <div>
                    <div class="flex flex-row gap-x-2 items-center">
                      <Checkbox v-bind="componentField">
                      </Checkbox>
                      <div class="text-sm [&>a]:text-primary flex flex-row items-center gap-x-1">
                        <div>
                          下次自动登录
                        </div>
                        <Info class="size-4 bg-"></Info>
                      </div>
                    </div>
                  </div>
                </FormControl>
                <!-- <FormDescription>登录密码</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>

            <!-- <div>{{ loginMessage }}</div> -->
            <div class="flex flex-row justify-center items-center gap-x-4">
              <Button class="hover:cursor-pointer flex-1" type="submit" :disabled="isLoginLoading">
                <Loader2 v-if="isLoginLoading" class="size-4 animate-spin" />
                登录
              </Button>
              <!-- <Button class="hover:cursor-pointer" type="button" variant="secondary"
                @click="router.push('/register')">注册</Button> -->
            </div>
          </form>
          <div class="flex flex-row items-center justify-center gap-x-4 [&>Button]:hover:cursor-pointer">
            <Button variant="ghost">
              忘记密码
            </Button>
            <span class="text-gray-300">|</span>
            <Button variant="ghost" @click="router.push('/register')">
              新用户注册
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>