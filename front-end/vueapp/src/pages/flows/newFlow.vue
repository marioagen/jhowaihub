<template>
    <FlowComponent 
        :stepOrder="routeStepOrder"
        :phase="routePhase"
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
            routePhase() {
                return parseInt(this.$route.params.phase);
            }
        },
    }
</script>