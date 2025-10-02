<template>
    <FlowComponent 
        :stepOrder="routeStepOrder"
    />
</template>

<script>
    import FlowComponent from '@/components/flow/FlowComponent.vue';
    export default {
        name: "EditFlow",
        components: {
            FlowComponent
        },
        computed: {
            routeStepOrder() {
                return parseInt(this.$route.params.stepOrder);
            },
        },
    }
</script>