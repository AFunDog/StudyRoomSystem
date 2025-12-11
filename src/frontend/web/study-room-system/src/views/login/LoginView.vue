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
import { loginSchema } from "@/lib/validation";
import { useForm, } from 'vee-validate';
import { onMounted, onUnmounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { LockKeyhole, Loader2, Info } from 'lucide-vue-next';
import { restartHubConnection } from '@/lib/api/hubConnection';
import { useConfig } from '@/lib/config';
import { toast } from 'vue-sonner';
import { AxiosError } from 'axios';
import { authRequest } from '@/lib/api/authRequest';
import { Checkbox } from '@/components/ui/checkbox';
import { Tooltip, TooltipTrigger, TooltipContent, TooltipProvider } from '@/components/ui/tooltip'

const router = useRouter();

//引入登录校验规则
const formSchema = toTypedSchema(loginSchema);
const form = useForm({ validationSchema: formSchema });

const isLoginLoading = ref(false);
const onSubmit = form.handleSubmit(async (values) => {
  try {
    console.log(values);
    isLoginLoading.value = true;
    var res = await authRequest.login({ userName: values.userName, password: values.password });

    //改成 HttpOnly Cookie 后，后端依然会生成token，但不会再通过 res.token 返回给前端
    //前端 JS 无法访问这个 Cookie，但浏览器会自动在请求时带上它
    // const token = res.token;
    // const user = res.user as User;

    //统一由后端返回错误信息
    // if (!user) {
    //   toast.error('登录失败，用户名或密码错误');
    //   return;
    // }

    // 登录成功（200）
    console.log('登录成功1');
    if (res.status === 200) {
      console.log('登录成功2');
      restartHubConnection();
      toast.success(`欢迎回来,${res.data.user.userName}!`);
      router.push('/');
    }
  } catch (error) {
    if (error instanceof AxiosError) {
      const status = error.response?.status;
      console.log('登录失败，状态码：', status);

      if (status === 401) {
        toast.error('登录失败：用户名或密码错误');
      } else if (status === 500) {
        toast.error('登录失败：服务器内部错误，请稍后再试');
      } else {
        toast.error('登录失败：请检查网络并稍后重试');
      }
    } else {
      toast.error('登录失败：发生未知错误');
    }
    console.error(error);
  } finally {
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
          <form class="flex flex-col gap-y-4" @submit="onSubmit">
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
            <FormField 
              name="agreePolicy" 
              v-slot="{ componentField }"
              validation-trigger="onSubmit"
            >
              <FormItem>
                <FormControl>
                  <div class="flex flex-row gap-x-2 items-center">
                    <Checkbox
                      :checked="componentField.modelValue" 
                      @update:model-value="componentField.onChange"
                    />
                    <div class="text-sm [&>a]:text-primary">
                      我已阅读并同意
                      <a href="/privacy-policy" target="_blank">《隐私政策》</a>
                      和
                      <a href="/user-agreement" target="_blank">《用户协议》</a>
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
                        <!-- 信息图标：默认灰色，悬停变主题色，并显示提示 -->
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Info class="size-4 text-gray-400 hover:text-primary cursor-pointer" />
                            </TooltipTrigger>
                            <TooltipContent>
                              勾选后，登录状态保持7天；如不勾选则关闭浏览器即为退出
                            </TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
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
            <!-- TODO: 忘记密码功能 -->
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