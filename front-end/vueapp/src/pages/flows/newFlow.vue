<template>
    <FlowComponent :stepOrder="$route.params.stepOrder"/>
</template>

<script>
    import FlowComponent from '@/components/flow/FlowComponent.vue';
    export default {
        name: "EditFlow",
        components: {
            FlowComponent
        },
    }
</script>