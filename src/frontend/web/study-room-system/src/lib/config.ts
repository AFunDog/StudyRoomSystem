import { ref, type Ref } from "vue";


interface Config {
    isBottomTagShow: Ref<boolean>;
}
const globalConfig = ref<Config>({ isBottomTagShow: ref(false) });
function useConfig() {
    return globalConfig.value;
}

export { useConfig };