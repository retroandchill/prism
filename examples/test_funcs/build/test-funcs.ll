; ModuleID = 'test-funcs'
source_filename = "test-funcs"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-i128:128-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-windows-msvc"

@test-funcs_V_N1a = global i32 1
@test-funcs_V_N1b = external global i32
@0 = private unnamed_addr constant [13 x i8] c"Hello world!\00", align 1
@llvm.global_ctors = appending global [1 x { i32, ptr, ptr }] [{ i32, ptr, ptr } { i32 65535, ptr @"test-funcs_F_N12<GlobalCtor>E_R_V", ptr null }]

define i32 @test-funcs_F_N3addE_I32_I32_R_I32(i32 %0, i32 %1) {
entry:
  %2 = add i32 %0, %1
  ret i32 %2
}

define double @test-funcs_F_N3addE_F32_I32_R_F64(float %0, i32 %1) {
entry:
  %2 = fpext float %0 to double
  %3 = uitofp i32 %1 to double
  %4 = fadd double %2, %3
  ret double %4
}

define i32 @test-funcs_F_N3maxE_I32_I32_R_I32(i32 %0, i32 %1) {
entry:
  %2 = alloca i32, align 4
  store i32 %0, ptr %2, align 4
  %3 = icmp sgt i32 %0, %1
  br i1 %3, label %cond.then, label %cond.else

cond.then:                                        ; preds = %entry
  store i32 %0, ptr %2, align 4
  br label %cond.merge

cond.else:                                        ; preds = %entry
  store i32 %1, ptr %2, align 4
  br label %cond.merge

cond.merge:                                       ; preds = %cond.else, %cond.then
  %4 = load i32, ptr %2, align 4
  ret i32 %4
}

define i32 @test-funcs_F_N7if_trueE_B_I32_I32_R_I32(i8 %0, i32 %1, i32 %2) {
entry:
  %3 = alloca i32, align 4
  store i8 %0, ptr %3, align 1
  %4 = icmp ne i8 %0, 0
  br i1 %4, label %cond.then, label %cond.else

cond.then:                                        ; preds = %entry
  store i32 %1, ptr %3, align 4
  br label %cond.merge

cond.else:                                        ; preds = %entry
  store i32 %2, ptr %3, align 4
  br label %cond.merge

cond.merge:                                       ; preds = %cond.else, %cond.then
  %5 = load i32, ptr %3, align 4
  ret i32 %5
}

define i32 @test-funcs_F_N10cast_checkE_I64_R_I32(i64 %0) {
entry:
  %1 = trunc i64 %0 to i32
  ret i32 %1
}

define i32 @test-funcs_F_N3minE_I32_I32_R_I32(i32 %0, i32 %1) {
entry:
  %2 = alloca i32, align 4
  store i32 %0, ptr %2, align 4
  %3 = icmp slt i32 %0, %1
  br i1 %3, label %then.cond, label %else.cond

then.cond:                                        ; preds = %entry
  store i32 %0, ptr %2, align 4
  %4 = load i32, ptr %2, align 4
  ret i32 %4

else.cond:                                        ; preds = %entry
  store i32 %1, ptr %2, align 4
  %5 = load i32, ptr %2, align 4
  ret i32 %5
}

define i32 @test-funcs_F_N15compound_assignE_I32_I32_R_I32(i32 %0, i32 %1) {
entry:
  %2 = add i32 %0, %1
  ret i32 %2
}

define i32 @test-funcs_F_N10while_loopE_I32_R_I32(i32 %0) {
entry:
  %1 = alloca i32, align 4
  store i32 0, ptr %1, align 4
  br label %loop.head

loop.head:                                        ; preds = %loop.body, %entry
  %2 = load i32, ptr %1, align 4
  %3 = icmp slt i32 %2, %0
  br i1 %3, label %loop.body, label %loop.cond.false

loop.body:                                        ; preds = %loop.head
  %4 = load i32, ptr %1, align 4
  %5 = load i32, ptr %1, align 4
  %6 = add i32 %5, 1
  store i32 %6, ptr %1, align 4
  br label %loop.head

loop.cond.false:                                  ; preds = %loop.head
  br label %loop.tail

loop.tail:                                        ; preds = %loop.cond.false
  %7 = load i32, ptr %1, align 4
  ret i32 %7
}

define i32 @test-funcs_F_N18unconditional_loopE_I32_R_I32(i32 %0) {
entry:
  %1 = alloca i32, align 4
  store i32 0, ptr %1, align 4
  br label %loop.head

loop.head:                                        ; preds = %cond.merge, %entry
  %2 = load i32, ptr %1, align 4
  %3 = icmp eq i32 %2, %0
  br i1 %3, label %then.cond, label %else.cond

then.cond:                                        ; preds = %loop.head
  br label %loop.tail

else.cond:                                        ; preds = %loop.head
  br label %cond.merge

cond.merge:                                       ; preds = %else.cond
  %4 = load i32, ptr %1, align 4
  %5 = load i32, ptr %1, align 4
  %6 = add i32 %5, 1
  store i32 %6, ptr %1, align 4
  br label %loop.head

loop.tail:                                        ; preds = %then.cond
  %7 = load i32, ptr %1, align 4
  ret i32 %7
}

define void @test-funcs_F_N8for_loopE_I32_R_V(i32 %0) {
entry:
  %1 = alloca i32, align 4
  store i32 0, ptr %1, align 4
  br label %loop.head

loop.head:                                        ; preds = %loop.increment, %entry
  %2 = load i32, ptr %1, align 4
  %3 = icmp slt i32 %2, %0
  br i1 %3, label %loop.body, label %loop.cond.false

loop.body:                                        ; preds = %loop.head
  br label %loop.increment

loop.cond.false:                                  ; preds = %loop.head
  br label %loop.tail

loop.increment:                                   ; preds = %loop.body
  %4 = load i32, ptr %1, align 4
  %5 = load i32, ptr %1, align 4
  %6 = add i32 %5, 1
  store i32 %6, ptr %1, align 4
  br label %loop.head

loop.tail:                                        ; preds = %loop.cond.false
  ret void
}

define void @test-funcs_F_N25nested_break_and_continueE_R_V() {
entry:
  %0 = alloca i32, align 4
  %1 = alloca i32, align 4
  %2 = alloca i8, align 1
  %3 = alloca i8, align 1
  store i32 0, ptr %0, align 4
  br label %loop.head

loop.head:                                        ; preds = %loop.increment10, %entry
  %4 = load i32, ptr %0, align 4
  %5 = icmp slt i32 %4, 10
  br i1 %5, label %loop.body, label %loop.cond.false

loop.body:                                        ; preds = %loop.head
  store i32 0, ptr %1, align 4
  br label %loop.head1

loop.cond.false:                                  ; preds = %loop.head
  br label %loop.tail11

loop.head1:                                       ; preds = %loop.increment, %loop.body
  %6 = load i32, ptr %1, align 4
  %7 = icmp slt i32 %6, 10
  br i1 %7, label %loop.body2, label %loop.cond.false3

loop.body2:                                       ; preds = %loop.head1
  %8 = load i32, ptr %0, align 4
  %9 = icmp eq i32 %8, 5
  br i1 %9, label %eval.right, label %eval.skip

loop.cond.false3:                                 ; preds = %loop.head1
  br label %loop.tail

then.cond:                                        ; preds = %eval.merge
  br label %loop.increment10

eval.right:                                       ; preds = %loop.body2
  %10 = load i32, ptr %1, align 4
  %11 = icmp eq i32 %10, 5
  store i1 %11, ptr %2, align 1
  br label %eval.merge

eval.skip:                                        ; preds = %loop.body2
  store i8 0, ptr %2, align 1
  br label %eval.merge

eval.merge:                                       ; preds = %eval.skip, %eval.right
  %12 = load i8, ptr %2, align 1
  %13 = icmp ne i8 %12, 0
  br i1 %13, label %then.cond, label %else.cond

else.cond:                                        ; preds = %eval.merge
  br label %cond.merge

cond.merge:                                       ; preds = %else.cond
  %14 = load i32, ptr %0, align 4
  %15 = icmp eq i32 %14, 5
  br i1 %15, label %eval.right5, label %eval.skip6

then.cond4:                                       ; preds = %eval.merge7
  br label %loop.tail11

eval.right5:                                      ; preds = %cond.merge
  %16 = load i32, ptr %1, align 4
  %17 = icmp eq i32 %16, 7
  store i1 %17, ptr %3, align 1
  br label %eval.merge7

eval.skip6:                                       ; preds = %cond.merge
  store i8 0, ptr %3, align 1
  br label %eval.merge7

eval.merge7:                                      ; preds = %eval.skip6, %eval.right5
  %18 = load i8, ptr %3, align 1
  %19 = icmp ne i8 %18, 0
  br i1 %19, label %then.cond4, label %else.cond8

else.cond8:                                       ; preds = %eval.merge7
  br label %cond.merge9

cond.merge9:                                      ; preds = %else.cond8
  br label %loop.increment

loop.increment:                                   ; preds = %cond.merge9
  %20 = load i32, ptr %1, align 4
  %21 = load i32, ptr %1, align 4
  %22 = add i32 %21, 1
  store i32 %22, ptr %1, align 4
  br label %loop.head1

loop.tail:                                        ; preds = %loop.cond.false3
  br label %loop.increment10

loop.increment10:                                 ; preds = %loop.tail, %then.cond
  %23 = load i32, ptr %0, align 4
  %24 = load i32, ptr %0, align 4
  %25 = add i32 %24, 1
  store i32 %25, ptr %0, align 4
  br label %loop.head

loop.tail11:                                      ; preds = %then.cond4, %loop.cond.false
  ret void
}

define void @test-funcs_F_N5printE_S__ref_R_V({ ptr, i64 } %0) {
entry:
  ret void
}

define void @test-funcs_F_N16add_by_referenceE_I32__mut__ref_I32_R_V(ptr %0, i32 %1) {
entry:
  %2 = load i32, ptr %0, align 4
  %3 = add i32 %2, %1
  store i32 %3, ptr %0, align 4
  ret void
}

define i32 @test-funcs_F_N19act_on_by_referenceE_I32_I32_R_I32(i32 %0, i32 %1) {
entry:
  %2 = alloca i32, align 4
  store i32 %0, ptr %2, align 4
  %3 = load i32, ptr %2, align 4
  %4 = add i32 %3, %1
  ret i32 %4
}

define void @test-funcs_F_N11print_maybeE_S__ref_I32_R_V({ ptr, i64 } %0, i32 %1) {
entry:
  %2 = icmp ne i32 %1, 5
  br i1 %2, label %then.cond, label %else.cond

then.cond:                                        ; preds = %entry
  call void @test-funcs_F_N5printE_S__ref_R_V({ ptr, i64 } %0)
  br label %after.call

after.call:                                       ; preds = %then.cond
  br label %cond.merge

else.cond:                                        ; preds = %entry
  br label %cond.merge

cond.merge:                                       ; preds = %else.cond, %after.call
  ret void
}

define i32 @test-funcs_F_N9get_indexE_I32__slice__ref_UZ_R_I32({ ptr, i64 } %0, i64 %1) {
entry:
  %2 = getelementptr i32, { ptr, i64 } %0, i64 %1
  %3 = load i32, { ptr, i64 } %2, align 4
  ret i32 %3
}

define i32 @test-funcs_F_N9get_indexE_I32__arr_4_UZ_R_I32(ptr byval([4 x i32]) %0, i64 %1) {
entry:
  %2 = alloca [4 x i32], align 4
  store ptr %0, ptr %2, align 8
  %3 = getelementptr [4 x i32], ptr %2, i64 %1
  %4 = load i32, ptr %3, align 4
  ret i32 %4
}

define i32 @test-funcs_F_N15get_second_itemE_I32__arr_4_R_I32(ptr byval([4 x i32]) %0) {
entry:
  %1 = alloca [4 x i32], align 4
  %slice = alloca [4 x i32], align 4
  store ptr %0, ptr %1, align 8
  call void @llvm.memcpy.p0.p0.i64(ptr align 4 %slice, ptr align 4 %1, i64 ptrtoint (ptr getelementptr ([4 x i32], ptr null, i32 1) to i64), i1 false)
  %2 = call i32 @test-funcs_F_N9get_indexE_I32__arr_4_UZ_R_I32(ptr %slice, i64 1)
  br label %after.call

after.call:                                       ; preds = %entry
  ret i32 %2
}

; Function Attrs: nocallback nofree nosync nounwind willreturn memory(argmem: readwrite)
declare void @llvm.memcpy.p0.p0.i64(ptr noalias writeonly captures(none), ptr noalias readonly captures(none), i64, i1 immarg) #0

define i32 @test-funcs_F_N22get_item_from_constantE_I32__arr_4_R_I32(ptr byval([4 x i32]) %0) {
entry:
  %1 = alloca [4 x i32], align 4
  %slice = alloca [4 x i32], align 4
  %start = call void @llvm.lifetime.start.p0(ptr %1)
  %2 = getelementptr [4 x i32], ptr %1, i64 0
  store i32 1, ptr %2, align 4
  %3 = getelementptr [4 x i32], ptr %1, i64 1
  store i32 2, ptr %3, align 4
  %4 = getelementptr [4 x i32], ptr %1, i64 2
  store i32 3, ptr %4, align 4
  %5 = getelementptr [4 x i32], ptr %1, i64 3
  store i32 4, ptr %5, align 4
  call void @llvm.memcpy.p0.p0.i64(ptr align 4 %slice, ptr align 4 %1, i64 ptrtoint (ptr getelementptr ([4 x i32], ptr null, i32 1) to i64), i1 false)
  %6 = call i32 @test-funcs_F_N9get_indexE_I32__arr_4_UZ_R_I32(ptr %slice, i64 2)
  br label %after.call

after.call:                                       ; preds = %entry
  %end = call void @llvm.lifetime.end.p0(ptr %1)
  ret i32 %6
}

; Function Attrs: nocallback nofree nosync nounwind willreturn memory(argmem: readwrite)
declare void @llvm.lifetime.start.p0(ptr captures(none)) #0

; Function Attrs: nocallback nofree nosync nounwind willreturn memory(argmem: readwrite)
declare void @llvm.lifetime.end.p0(ptr captures(none)) #0

define i32 @test-funcs_F_N26get_index_from_large_arrayE_I32__arr_5_R_I32(ptr byval([5 x i32]) %0) {
entry:
  %1 = alloca [5 x i32], align 4
  store ptr %0, ptr %1, align 8
  %2 = call i32 @test-funcs_F_N9get_indexE_I32__slice__ref_UZ_R_I32(ptr %1, i64 2)
  br label %after.call

after.call:                                       ; preds = %entry
  ret i32 %2
}

define i32 @test-funcs_F_N35materialize_temporary_into_functionE_R_I32() {
entry:
  %0 = alloca [3 x i32], align 4
  %start = call void @llvm.lifetime.start.p0(ptr %0)
  %1 = getelementptr [3 x i32], ptr %0, i64 0
  store i32 1, ptr %1, align 4
  %2 = getelementptr [3 x i32], ptr %0, i64 1
  store i32 2, ptr %2, align 4
  %3 = getelementptr [3 x i32], ptr %0, i64 2
  store i32 3, ptr %3, align 4
  %4 = call i32 @test-funcs_F_N9get_indexE_I32__slice__ref_UZ_R_I32(ptr %0, i64 0)
  br label %after.call

after.call:                                       ; preds = %entry
  %end = call void @llvm.lifetime.end.p0(ptr %0)
  ret i32 %4
}

define void @test-funcs_F_N17print_char_arraysE_R_V() {
entry:
  %0 = alloca [5 x i8], align 1
  %1 = alloca [5 x i8], align 1
  %start = call void @llvm.lifetime.start.p0(ptr %0)
  %2 = getelementptr [5 x i8], ptr %0, i64 0
  store i8 72, ptr %2, align 1
  %3 = getelementptr [5 x i8], ptr %0, i64 1
  store i8 101, ptr %3, align 1
  %4 = getelementptr [5 x i8], ptr %0, i64 2
  store i8 108, ptr %4, align 1
  %5 = getelementptr [5 x i8], ptr %0, i64 3
  store i8 108, ptr %5, align 1
  %6 = getelementptr [5 x i8], ptr %0, i64 4
  store i8 111, ptr %6, align 1
  call void @test-funcs_F_N5printE_S__ref_R_V(ptr %0)
  br label %after.call

after.call:                                       ; preds = %entry
  %end = call void @llvm.lifetime.end.p0(ptr %0)
  %start1 = call void @llvm.lifetime.start.p0(ptr %1)
  %7 = getelementptr [5 x i8], ptr %1, i64 0
  store i8 87, ptr %7, align 1
  %8 = getelementptr [5 x i8], ptr %1, i64 1
  store i8 111, ptr %8, align 1
  %9 = getelementptr [5 x i8], ptr %1, i64 2
  store i8 114, ptr %9, align 1
  %10 = getelementptr [5 x i8], ptr %1, i64 3
  store i8 108, ptr %10, align 1
  %11 = getelementptr [5 x i8], ptr %1, i64 4
  store i8 100, ptr %11, align 1
  call void @test-funcs_F_N5printE_S__ref_R_V(ptr %1)
  br label %after.call2

after.call2:                                      ; preds = %after.call
  %end3 = call void @llvm.lifetime.end.p0(ptr %1)
  ret void
}

define void @test-funcs_F_N16print_char_arrayE_C__slice__ref_R_V({ ptr, i64 } %0) {
entry:
  call void @test-funcs_F_N5printE_S__ref_R_V({ ptr, i64 } %0)
  br label %after.call

after.call:                                       ; preds = %entry
  ret void
}

define { i8, i32 } @test-funcs_F_N15nullable_checksE_B_R_I32__nullable(i8 %0) {
entry:
  %1 = alloca { i8, i32 }, align 4
  store { i8, i32 } zeroinitializer, ptr %1, align 4
  %2 = icmp ne i8 %0, 0
  %3 = select i1 %2, { i8, i32 } { i8 1, i32 3 }, { i8, i32 } zeroinitializer
  store { i8, i32 } { i8 1, i32 4 }, ptr %1, align 4
  ret { i8, i32 } %3
}

define i32 @test-funcs_F_N25nullable_arithmetic_checkE_I32__nullable_I32__nullable_R_I32({ i8, i32 } %0, { i8, i32 } %1) {
entry:
  %2 = alloca i32, align 4
  store { i8, i32 } %0, ptr %2, align 4
  %3 = alloca { i8, i32 }, align 4
  store { i8, i32 } %0, ptr %3, align 4
  %4 = alloca { i8, i32 }, align 4
  store { i8, i32 } %1, ptr %4, align 4
  %5 = alloca i8, align 1
  %6 = load i8, ptr %3, align 1
  %7 = icmp eq i8 %6, 0
  br i1 %7, label %eval.right, label %eval.skip

eval.right:                                       ; preds = %entry
  %8 = load i8, ptr %4, align 1
  %9 = icmp eq i8 %8, 0
  store i1 %9, ptr %5, align 1
  br label %eval.merge

eval.skip:                                        ; preds = %entry
  store i8 0, ptr %5, align 1
  br label %eval.merge

eval.merge:                                       ; preds = %eval.skip, %eval.right
  %10 = load i8, ptr %5, align 1
  %11 = icmp ne i8 %10, 0
  br i1 %11, label %cond.then, label %cond.else

cond.then:                                        ; preds = %eval.merge
  %12 = getelementptr inbounds nuw { i8, i32 }, ptr %3, i32 0, i32 1
  %13 = load i32, ptr %12, align 4
  %14 = getelementptr inbounds nuw { i8, i32 }, ptr %4, i32 0, i32 1
  %15 = load i32, ptr %14, align 4
  %16 = add i32 %13, %15
  store i32 %16, ptr %2, align 4
  br label %cond.merge

cond.else:                                        ; preds = %eval.merge
  store i32 0, ptr %2, align 4
  br label %cond.merge

cond.merge:                                       ; preds = %cond.else, %cond.then
  %17 = load i32, ptr %2, align 4
  ret i32 %17
}

define void @test-funcs_F_N25handle_nullable_referenceE_I32__ref__nullable_R_V(ptr %0) {
entry:
  ret void
}

define void @test-funcs_F_N20handle_nullable_spanE_S__ref__nullable_R_V({ ptr, i64 } %0) {
entry:
  %1 = alloca { ptr, i64 }, align 8
  store { ptr, i64 } %0, ptr %1, align 8
  %2 = load ptr, ptr %1, align 8
  %3 = icmp eq ptr %2, null
  br i1 %3, label %then.cond, label %else.cond

then.cond:                                        ; preds = %entry
  %4 = load { ptr, i64 }, ptr %1, align 8
  call void @test-funcs_F_N5printE_S__ref_R_V({ ptr, i64 } %4)
  br label %after.call

after.call:                                       ; preds = %then.cond
  br label %cond.merge

else.cond:                                        ; preds = %entry
  br label %cond.merge

cond.merge:                                       ; preds = %else.cond, %after.call
  ret void
}

define void @test-funcs_F_N13optional_boolE_B__nullable_R_V(i8 %0) {
entry:
  %1 = icmp eq i8 %0, 2
  br i1 %1, label %then.cond, label %else.cond

then.cond:                                        ; preds = %entry
  call void @test-funcs_F_N5printE_S__ref_R_V({ ptr, i64 } { ptr @0, i64 12 })
  br label %after.call

after.call:                                       ; preds = %then.cond
  br label %cond.merge

else.cond:                                        ; preds = %entry
  br label %cond.merge

cond.merge:                                       ; preds = %else.cond, %after.call
  ret void
}

define void @"test-funcs_F_N12<GlobalCtor>E_R_V"() {
entry:
  %0 = load i32, ptr @test-funcs_V_N1a, align 4
  %1 = add i32 %0, 2
  store i32 %1, ptr @test-funcs_V_N1b, align 4
  ret void
}

attributes #0 = { nocallback nofree nosync nounwind willreturn memory(argmem: readwrite) }
