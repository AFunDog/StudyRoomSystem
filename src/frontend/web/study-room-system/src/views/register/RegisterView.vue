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
import { LockKeyhole, Eye, EyeOff } from 'lucide-vue-next';

const router = useRouter();
const schema = z.object({
  userName: z.string({ required_error: '请输入用户名' }).min(4, '请输入用户名'),
  campusId: z.string({ required_error: '请输入学号/工号' }),
  phone: z.string({ required_error: '请输入手机号' }).regex(/^1[3-9]\d{9}$/, '请输入有效的手机号'),
  password: z.string({ required_error: '请输入密码' }).min(6, '请输入密码'),
  confirmPassword: z.string({ required_error: '请输入确认密码' }).min(6, '请输入确认密码')
}).refine((data) => data.password === data.confirmPassword, {
  path: ['confirmPassword'],
  message: '确认密码与密码不一致'
});
const formSchema = toTypedSchema(schema)
const registerMessage = ref('');
const isShowPassword = ref(false);

async function onSubmit(values: GenericObject) {
  try {
    console.log(values);
    var res = await http.post('/api/v1/auth/register', {
      userName: values.userName,
      password: values.password,
      campusId: values.campusId,
      phone: values.phone
    })

    const token = res.data.token;
    const user = res.data.user as User;

    if (!res.data.data) {
      registerMessage.value = '注册失败';
      return;
    }

    router.push('/login');

  }
  catch (error) {
    registerMessage.value = '注册失败';
    console.log(error);
  }
}

</script>
<template>
  <div class="flex flex-col items-center justify-center h-screen">
    <div class="w-5/6 max-w-xl">
      <Card>
        <CardHeader>
          <div class="text-2xl tracking-widest">注册</div>
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
            <FormField v-slot="{ componentField }" name="campusId">
              <FormItem>
                <FormLabel>学号/工号</FormLabel>
                <FormControl>
                  <Input v-bind="componentField" placeholder="请输入学号/工号" />
                </FormControl>
                <!-- <FormDescription>用户注册时的名称</FormDescription> -->
                <FormMessage />
              </FormItem>
            </FormField>
            <FormField v-slot="{ componentField }" name="phone">
              <FormItem>
                <FormLabel>手机号</FormLabel>
                <FormControl>
                  <Input v-bind="componentField" placeholder="请输入手机号" />
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
                    <Input v-bind="componentField" type="password" autocomplete="new-password" placeholder="请输入密码"
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
            <FormField v-slot="{ componentField }" name="confirmPassword"> 
              <FormItem>
                <FormLabel>确认密码</FormLabel>
                <FormControl>
                  <div class="flex flex-row items-center relative">
                    <div class="absolute flex flex-row justify-center items-center w-10">
                      <LockKeyhole class="size-4"></LockKeyhole>
                    </div>
                    <Input v-bind="componentField" type="password" autocomplete="new-password" placeholder="请输入确认密码"
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
            <div>{{ registerMessage }}</div>
            <div class="flex flex-row justify-center items-center gap-x-4">
              <Button class="hover:cursor-pointer" type="button" variant="secondary" @click="router.back()">返回</Button>
              <Button class="hover:cursor-pointer" type="submit">注册</Button>
            </div>
          </Form>
        </CardContent>
      </Card>
    </div>
  </div>
</template>