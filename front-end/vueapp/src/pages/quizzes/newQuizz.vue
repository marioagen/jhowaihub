<template>
    <QuizzesForm 
        :edit="false"
    />
</template>

<script>
    import QuizzesForm from '@/components/quizzes/QuizzesForm.vue';
    export default {
        name: "EditQuizz",
        components: {
            QuizzesForm
        },
    }
</script>